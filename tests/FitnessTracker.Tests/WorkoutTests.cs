using FitnessTracker.Application;
using FitnessTracker.Domain;
using FitnessTracker.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;

namespace FitnessTracker.Tests
{
    [TestClass]
    public class WorkoutTests
    {
        private IWorkoutRepository _workoutRepo;
        private IUserRepository _userRepo;
        private WorkoutService _service;
        private WorkoutQueryService _queryService;
        private User _user;

        [TestInitialize]
        public void Setup()
        {
            _workoutRepo = new InMemoryWorkoutRepository();
            _userRepo = new InMemoryUserRepository();
            _service = new WorkoutService(_workoutRepo, _userRepo);
            _queryService = new WorkoutQueryService(_workoutRepo);
            _user = new User("TestUser", 25, 70.0);
            _userRepo.Save(_user);
        }

        [TestMethod]
        public void StartWorkout_ValidUser_ReturnsWorkout()
        {
            var workout = _service.StartWorkout(_user.Id, "Test", WorkoutType.Cardio);
            Assert.IsNotNull(workout);
            Assert.AreEqual("Test", workout.Title);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartWorkout_NonExistentUser_Throws()
        {
            _service.StartWorkout(Guid.NewGuid(), "Ghost", WorkoutType.Strength);
        }

        [TestMethod]
        public void AddExercise_ToActiveWorkout_IncreasesCount()
        {
            var workout = _service.StartWorkout(_user.Id, "Strength", WorkoutType.Strength);
            _service.AddExercise(workout.Id, "Deadlift", 3, 5, 120.0, 200);
            Assert.AreEqual(1, workout.Exercises.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void AddExercise_ToCompletedWorkout_Throws()
        {
            var workout = _service.StartWorkout(_user.Id, "Done", WorkoutType.Flexibility);
            _service.CompleteWorkout(workout.Id);
            _service.AddExercise(workout.Id, "Stretch", 1, 1, 0, 60);
        }

        [TestMethod]
        public void CompleteWorkout_SetsIsCompleted()
        {
            var workout = _service.StartWorkout(_user.Id, "HIIT", WorkoutType.HIIT);
            _service.CompleteWorkout(workout.Id);
            Assert.IsTrue(workout.IsCompleted);
        }

        [TestMethod]
        public void GetUserHistory_ReturnsOnlyUserWorkouts()
        {
            _service.StartWorkout(_user.Id, "W1", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "W2", WorkoutType.Strength);
            var other = new User("Other", 30, 80.0);
            _userRepo.Save(other);
            _service.StartWorkout(other.Id, "W3", WorkoutType.HIIT);
            Assert.AreEqual(2, _service.GetUserHistory(_user.Id).Count());
        }

        [TestMethod]
        public void TotalDurationSeconds_SumsAllExercises()
        {
            var workout = _service.StartWorkout(_user.Id, "Test", WorkoutType.Strength);
            _service.AddExercise(workout.Id, "A", 1, 1, 0, 100);
            _service.AddExercise(workout.Id, "B", 1, 1, 0, 200);
            Assert.AreEqual(300, workout.TotalDurationSeconds());
        }
    }

    [TestClass]
    public class DomainTests
    {
        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_EmptyName_Throws()
        {
            new User("", 25, 70.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_NegativeAge_Throws()
        {
            new User("Test", -1, 70.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_NegativeWeight_Throws()
        {
            new User("Test", 25, -10.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExercise_NegativeSets_Throws()
        {
            new Exercise("Squat", -1, 10, 50.0, 60);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CompleteWorkout_BeforeStart_Throws()
        {
            var workout = new Workout(Guid.NewGuid(), "Test", WorkoutType.Cardio, DateTime.UtcNow);
            workout.Complete(DateTime.UtcNow.AddHours(-1));
        }
    }

    [TestClass]
    public class CalorieCalculatorTests
    {
        private User _user;

        [TestInitialize]
        public void Setup()
        {
            _user = new User("Test", 25, 70.0);
        }

        [TestMethod]
        public void StrengthCalculator_ReturnsPositive()
        {
            var workout = new Workout(_user.Id, "Strength", WorkoutType.Strength, DateTime.UtcNow);
            var exercise = new Exercise("Bench", 4, 10, 80.0, 120);
            workout.AddExercise(exercise);
            var calculator = new StrengthCalorieCalculator();
            var calories = calculator.Calculate(workout, _user);
            Assert.IsTrue(calories > 0);
        }

        [TestMethod]
        public void CardioCalculator_ReturnsPositive()
        {
            var workout = new Workout(_user.Id, "Run", WorkoutType.Cardio, DateTime.UtcNow);
            var exercise = new Exercise("Treadmill", 1, 1, 0, 1800);
            workout.AddExercise(exercise);
            var calculator = new CardioCalorieCalculator();
            var calories = calculator.Calculate(workout, _user);
            Assert.IsTrue(calories > 0);
        }

        [TestMethod]
        public void DefaultCalculator_ZeroDuration_ReturnsZero()
        {
            var workout = new Workout(_user.Id, "Empty", WorkoutType.HIIT, DateTime.UtcNow);
            var calculator = new DefaultCalorieCalculator();
            var calories = calculator.Calculate(workout, _user);
            Assert.AreEqual(0.0, calories, 0.001);
        }
    }

    [TestClass]
    public class QueryServiceTests
    {
        private IWorkoutRepository _repo;
        private WorkoutQueryService _query;
        private User _user;

        [TestInitialize]
        public void Setup()
        {
            _repo = new InMemoryWorkoutRepository();
            _query = new WorkoutQueryService(_repo);
            _user = new User("Test", 25, 70.0);
        }

        private void CreateWorkout(string title, WorkoutType type, bool completed)
        {
            var workout = new Workout(_user.Id, title, type, DateTime.UtcNow);
            workout.AddExercise(new Exercise(title, 1, 1, 0, 60));
            if (completed)
                workout.Complete(DateTime.UtcNow);
            _repo.Save(workout);
        }

        [TestMethod]
        public void GetCompletedByUser_ReturnsOnlyCompleted()
        {
            CreateWorkout("W1", WorkoutType.Cardio, true);
            CreateWorkout("W2", WorkoutType.Strength, false);
            var completed = _query.GetCompletedByUser(_user.Id).ToList();
            Assert.AreEqual(1, completed.Count);
        }

        [TestMethod]
        public void GetByType_ReturnsCorrectType()
        {
            CreateWorkout("Cardio Day", WorkoutType.Cardio, true);
            CreateWorkout("Strength Day", WorkoutType.Strength, true);
            var cardio = _query.GetByType(_user.Id, WorkoutType.Cardio).ToList();
            Assert.AreEqual(1, cardio.Count);
            Assert.AreEqual(WorkoutType.Cardio, cardio[0].Type);
        }

        [TestMethod]
        public void GetWorkoutCountByType_GroupsCorrectly()
        {
            CreateWorkout("C1", WorkoutType.Cardio, true);
            CreateWorkout("C2", WorkoutType.Cardio, true);
            CreateWorkout("S1", WorkoutType.Strength, true);
            var dict = _query.GetWorkoutCountByType(_user.Id);
            Assert.AreEqual(2, dict[WorkoutType.Cardio]);
            Assert.AreEqual(1, dict[WorkoutType.Strength]);
        }

        [TestMethod]
        public void GetTotalDurationMinutes_OnlyCompleted()
        {
            var w1 = new Workout(_user.Id, "W1", WorkoutType.Strength, DateTime.UtcNow);
            w1.AddExercise(new Exercise("A", 1, 1, 0, 120));
            w1.Complete(DateTime.UtcNow);
            _repo.Save(w1);

            var w2 = new Workout(_user.Id, "W2", WorkoutType.Cardio, DateTime.UtcNow);
            w2.AddExercise(new Exercise("B", 1, 1, 0, 600));
            _repo.Save(w2);

            var total = _query.GetTotalDurationMinutes(_user.Id);
            Assert.AreEqual(2.0, total, 0.01);
        }

        [TestMethod]
        public void GetLongestWorkout_ReturnsCorrect()
        {
            var w1 = new Workout(_user.Id, "Short", WorkoutType.Strength, DateTime.UtcNow);
            w1.AddExercise(new Exercise("A", 1, 1, 0, 60));
            w1.Complete(DateTime.UtcNow);
            _repo.Save(w1);

            var w2 = new Workout(_user.Id, "Long", WorkoutType.Cardio, DateTime.UtcNow);
            w2.AddExercise(new Exercise("B", 1, 1, 0, 3600));
            w2.Complete(DateTime.UtcNow);
            _repo.Save(w2);

            var longest = _query.GetLongestWorkout(_user.Id);
            Assert.AreEqual("Long", longest.Title);
        }
    }

    [TestClass]
    public class IntegrationTests
    {
        private string _tempDir;

        [TestInitialize]
        public void Setup()
        {
            _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_tempDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_tempDir))
                Directory.Delete(_tempDir, true);
        }

        [TestMethod]
        public void Persistence_WorkoutSavedAndReloaded()
        {
            var workoutPath = Path.Combine(_tempDir, "workouts.json");
            var userPath = Path.Combine(_tempDir, "users.json");

            var userRepo1 = new FileUserRepository(userPath);
            var workoutRepo1 = new FileWorkoutRepository(workoutPath);
            var userService1 = new UserService(userRepo1);
            var user = userService1.Register("TestUser", 25, 70.0);

            var service1 = new WorkoutService(workoutRepo1, userRepo1);
            var workout = service1.StartWorkout(user.Id, "Test Workout", WorkoutType.Strength);
            service1.AddExercise(workout.Id, "Squat", 3, 10, 100.0, 120);
            service1.CompleteWorkout(workout.Id);

            var workoutRepo2 = new FileWorkoutRepository(workoutPath);
            var reloaded = workoutRepo2.GetById(workout.Id);

            Assert.IsNotNull(reloaded);
            Assert.AreEqual("Test Workout", reloaded.Title);
            Assert.IsTrue(reloaded.IsCompleted);
            Assert.AreEqual(1, reloaded.Exercises.Count);
        }

        [TestMethod]
        public void Persistence_MissingFile_ReturnsEmpty()
        {
            var repo = new FileWorkoutRepository(Path.Combine(_tempDir, "nonexistent.json"));
            var all = repo.GetAll().ToList();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public void Persistence_CorruptedFile_ReturnsEmpty()
        {
            var path = Path.Combine(_tempDir, "corrupted.json");
            File.WriteAllText(path, "NOT JSON{{{");
            var repo = new FileWorkoutRepository(path);
            var all = repo.GetAll().ToList();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public void UserPersistence_SaveAndReload()
        {
            var userPath = Path.Combine(_tempDir, "users.json");

            var userRepo1 = new FileUserRepository(userPath);
            var userService1 = new UserService(userRepo1);
            var user = userService1.Register("SaveUser", 22, 65.0);

            var userRepo2 = new FileUserRepository(userPath);
            var reloaded = userRepo2.GetById(user.Id);

            Assert.IsNotNull(reloaded);
            Assert.AreEqual("SaveUser", reloaded.Name);
        }

        [TestMethod]
        public void Persistence_MultipleWorkouts_AllReloaded()
        {
            var workoutPath = Path.Combine(_tempDir, "multi_workouts.json");
            var userPath = Path.Combine(_tempDir, "multi_users.json");

            var userRepo1 = new FileUserRepository(userPath);
            var workoutRepo1 = new FileWorkoutRepository(workoutPath);
            var userService1 = new UserService(userRepo1);
            var user = userService1.Register("MultiUser", 25, 70.0);

            var service1 = new WorkoutService(workoutRepo1, userRepo1);
            for (int i = 0; i < 3; i++)
            {
                var w = service1.StartWorkout(user.Id, $"W{i}", WorkoutType.Cardio);
                service1.CompleteWorkout(w.Id);
            }

            var workoutRepo2 = new FileWorkoutRepository(workoutPath);
            var history = workoutRepo2.GetByUserId(user.Id).ToList();
            Assert.AreEqual(3, history.Count);
        }

        [TestMethod]
        public void Persistence_EmptyDataFile_ReturnsEmpty()
        {
            var path = Path.Combine(_tempDir, "empty.json");
            File.WriteAllText(path, "[]");
            var repo = new FileWorkoutRepository(path);
            var all = repo.GetAll().ToList();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public void Persistence_WorkoutWithNoExercises_SavesAndReloads()
        {
            var workoutPath = Path.Combine(_tempDir, "no_exercises.json");
            var userPath = Path.Combine(_tempDir, "no_exercises_user.json");

            var userRepo1 = new FileUserRepository(userPath);
            var workoutRepo1 = new FileWorkoutRepository(workoutPath);
            var userService1 = new UserService(userRepo1);
            var user = userService1.Register("EmptyUser", 25, 70.0);

            var service1 = new WorkoutService(workoutRepo1, userRepo1);
            var workout = service1.StartWorkout(user.Id, "Empty", WorkoutType.Cardio);
            service1.CompleteWorkout(workout.Id);

            var workoutRepo2 = new FileWorkoutRepository(workoutPath);
            var reloaded = workoutRepo2.GetById(workout.Id);

            Assert.IsNotNull(reloaded);
            Assert.AreEqual(0, reloaded.Exercises.Count);
            Assert.IsTrue(reloaded.IsCompleted);
        }

        [TestMethod]
        public void FullCycle_MultipleUsers_DataIsolated()
        {
            var workoutPath = Path.Combine(_tempDir, "isolated_workouts.json");
            var userPath = Path.Combine(_tempDir, "isolated_users.json");

            var userRepo = new FileUserRepository(userPath);
            var workoutRepo = new FileWorkoutRepository(workoutPath);
            var userService = new UserService(userRepo);

            var user1 = userService.Register("User1", 25, 70.0);
            var user2 = userService.Register("User2", 30, 80.0);

            var service = new WorkoutService(workoutRepo, userRepo);
            var w1 = service.StartWorkout(user1.Id, "U1 Workout", WorkoutType.Cardio);
            service.CompleteWorkout(w1.Id);
            var w2 = service.StartWorkout(user2.Id, "U2 Workout", WorkoutType.Strength);
            service.CompleteWorkout(w2.Id);

            var user1Workouts = workoutRepo.GetByUserId(user1.Id).ToList();
            var user2Workouts = workoutRepo.GetByUserId(user2.Id).ToList();

            Assert.AreEqual(1, user1Workouts.Count);
            Assert.AreEqual("U1 Workout", user1Workouts[0].Title);
            Assert.AreEqual(1, user2Workouts.Count);
            Assert.AreEqual("U2 Workout", user2Workouts[0].Title);
        }
    }
}