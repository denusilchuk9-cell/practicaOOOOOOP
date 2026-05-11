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
        public void AddExercise_ToActiveWorkout_IncreasesCount()
        {
            var workout = _service.StartWorkout(_user.Id, "Strength", WorkoutType.Strength);
            _service.AddExercise(workout.Id, "Deadlift", 3, 5, 120.0, 200);
            Assert.AreEqual(1, workout.Exercises.Count);
        }

        [TestMethod]
        public void CompleteWorkout_SetsIsCompleted()
        {
            var workout = _service.StartWorkout(_user.Id, "HIIT", WorkoutType.HIIT);
            _service.CompleteWorkout(workout.Id);
            Assert.IsTrue(workout.IsCompleted);
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
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartWorkout_NonExistentUser_Throws()
        {
            _service.StartWorkout(Guid.NewGuid(), "Ghost", WorkoutType.Strength);
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

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateUser_EmptyName_Throws()
        {
            new User("", 25, 70.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExercise_NegativeSets_Throws()
        {
            new Exercise("Squat", -1, 10, 50.0, 60);
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
        public void Workout_MultipleExercises_CountCorrect()
        {
            var workout = _service.StartWorkout(_user.Id, "Big Day", WorkoutType.Strength);
            for (int i = 0; i < 5; i++)
                _service.AddExercise(workout.Id, $"Ex{i}", 3, 10, 50.0, 60);
            Assert.AreEqual(5, workout.Exercises.Count);
        }

        [TestMethod]
        public void GetCompletedByUser_ReturnsOnlyCompleted()
        {
            var w1 = _service.StartWorkout(_user.Id, "W1", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "W2", WorkoutType.Strength);
            _service.CompleteWorkout(w1.Id);
            var completed = _queryService.GetCompletedByUser(_user.Id).ToList();
            Assert.AreEqual(1, completed.Count);
            Assert.AreEqual("W1", completed[0].Title);
        }

        [TestMethod]
        public void GetByType_ReturnsCorrectType()
        {
            _service.StartWorkout(_user.Id, "Cardio Day", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "Strength Day", WorkoutType.Strength);
            var cardio = _queryService.GetByType(_user.Id, WorkoutType.Cardio).ToList();
            Assert.AreEqual(1, cardio.Count);
            Assert.AreEqual(WorkoutType.Cardio, cardio[0].Type);
        }

        [TestMethod]
        public void GetWorkoutCountByType_GroupsCorrectly()
        {
            _service.StartWorkout(_user.Id, "C1", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "C2", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "S1", WorkoutType.Strength);
            var dict = _queryService.GetWorkoutCountByType(_user.Id);
            Assert.AreEqual(2, dict[WorkoutType.Cardio]);
            Assert.AreEqual(1, dict[WorkoutType.Strength]);
        }

        [TestMethod]
        public void GetTotalDurationMinutes_OnlyCompleted()
        {
            var w1 = _service.StartWorkout(_user.Id, "W1", WorkoutType.Strength);
            _service.AddExercise(w1.Id, "A", 1, 1, 0, 120);
            _service.CompleteWorkout(w1.Id);
            var w2 = _service.StartWorkout(_user.Id, "W2", WorkoutType.Cardio);
            _service.AddExercise(w2.Id, "B", 1, 1, 0, 600);
            var total = _queryService.GetTotalDurationMinutes(_user.Id);
            Assert.AreEqual(2.0, total, 0.01);
        }

        [TestMethod]
        public void StrengthCalorieCalculator_ReturnsPositive()
        {
            var workout = _service.StartWorkout(_user.Id, "Strength", WorkoutType.Strength);
            _service.AddExercise(workout.Id, "Bench", 4, 10, 80.0, 120);
            var calc = new StrengthCalorieCalculator();
            var cal = calc.Calculate(workout, _user);
            Assert.IsTrue(cal > 0);
        }

        [TestMethod]
        public void CardioCalorieCalculator_ReturnsPositive()
        {
            var workout = _service.StartWorkout(_user.Id, "Run", WorkoutType.Cardio);
            _service.AddExercise(workout.Id, "Treadmill", 1, 1, 0, 1800);
            var calc = new CardioCalorieCalculator();
            var cal = calc.Calculate(workout, _user);
            Assert.IsTrue(cal > 0);
        }

        [TestMethod]
        public void DefaultCalorieCalculator_ZeroDuration_ReturnsZero()
        {
            var workout = _service.StartWorkout(_user.Id, "Empty", WorkoutType.HIIT);
            var calc = new DefaultCalorieCalculator();
            var cal = calc.Calculate(workout, _user);
            Assert.AreEqual(0.0, cal, 0.001);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CompleteWorkout_BeforeStart_Throws()
        {
            var workout = new Workout(_user.Id, "Test", WorkoutType.Cardio, DateTime.UtcNow);
            workout.Complete(DateTime.UtcNow.AddHours(-1));
        }

        [TestMethod]
        public void GetLongestWorkout_ReturnsCorrect()
        {
            var w1 = _service.StartWorkout(_user.Id, "Short", WorkoutType.Strength);
            _service.AddExercise(w1.Id, "A", 1, 1, 0, 60);
            _service.CompleteWorkout(w1.Id);

            var w2 = _service.StartWorkout(_user.Id, "Long", WorkoutType.Cardio);
            _service.AddExercise(w2.Id, "B", 1, 1, 0, 3600);
            _service.CompleteWorkout(w2.Id);

            var longest = _queryService.GetLongestWorkout(_user.Id);
            Assert.AreEqual("Long", longest.Title);
        }

        [TestMethod]
        public void UserService_Register_SavesUser()
        {
            var userService = new UserService(_userRepo);
            var u = userService.Register("NewUser", 30, 75.0);
            var found = _userRepo.GetById(u.Id);
            Assert.IsNotNull(found);
            Assert.AreEqual("NewUser", found.Name);
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

        private (WorkoutService, UserService, WorkoutQueryService, User) BuildServices()
        {
            var workoutRepo = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var userRepo = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var workoutService = new WorkoutService(workoutRepo, userRepo);
            var userService = new UserService(userRepo);
            var queryService = new WorkoutQueryService(workoutRepo);
            var user = userService.Register("IntegrationUser", 28, 72.0);
            return (workoutService, userService, queryService, user);
        }

        [TestMethod]
        public void Persistence_WorkoutSavedAndReloaded()
        {
            Guid workoutId;
            Guid userId;

            var userRepo1 = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var workoutRepo1 = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var service1 = new WorkoutService(workoutRepo1, userRepo1);
            var userService1 = new UserService(userRepo1);
            var user1 = userService1.Register("PersistUser", 25, 70.0);
            userId = user1.Id;
            var w = service1.StartWorkout(userId, "Persisted Workout", WorkoutType.Strength);
            service1.AddExercise(w.Id, "Squat", 3, 10, 100.0, 180);
            service1.CompleteWorkout(w.Id);
            workoutId = w.Id;

            var userRepo2 = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var workoutRepo2 = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var service2 = new WorkoutService(workoutRepo2, userRepo2);
            var reloaded = workoutRepo2.GetById(workoutId);

            Assert.IsNotNull(reloaded);
            Assert.AreEqual("Persisted Workout", reloaded.Title);
            Assert.IsTrue(reloaded.IsCompleted);
            Assert.AreEqual(1, reloaded.Exercises.Count);
        }

        [TestMethod]
        public void Persistence_MultipleWorkouts_AllReloaded()
        {
            var (service, userService, _, user) = BuildServices();
            for (int i = 0; i < 3; i++)
                service.StartWorkout(user.Id, $"Workout {i}", WorkoutType.Cardio);

            var workoutRepo2 = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var history = workoutRepo2.GetByUserId(user.Id).ToList();
            Assert.AreEqual(3, history.Count);
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
            var path = Path.Combine(_tempDir, "workouts.json");
            File.WriteAllText(path, "THIS IS NOT JSON {{{{");
            var repo = new FileWorkoutRepository(path);
            var all = repo.GetAll().ToList();
            Assert.AreEqual(0, all.Count);
        }

        [TestMethod]
        public void FullCycle_CreateAddComplete_PersistsCorrectly()
        {
            var (service, _, query, user) = BuildServices();
            var w = service.StartWorkout(user.Id, "Full Cycle", WorkoutType.HIIT);
            service.AddExercise(w.Id, "Burpee", 5, 20, 0, 300);
            service.CompleteWorkout(w.Id);

            var workoutRepo2 = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var userRepo2 = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var service2 = new WorkoutService(workoutRepo2, userRepo2);
            var history = service2.GetUserHistory(user.Id).ToList();

            Assert.AreEqual(1, history.Count);
            Assert.AreEqual("Full Cycle", history[0].Title);
            Assert.AreEqual(1, history[0].Exercises.Count);
        }

        [TestMethod]
        public void Analytics_AfterReload_CorrectStats()
        {
            var (service, _, _, user) = BuildServices();
            var w = service.StartWorkout(user.Id, "Stats Test", WorkoutType.Strength);
            service.AddExercise(w.Id, "Press", 3, 8, 60.0, 240);
            service.CompleteWorkout(w.Id);

            var workoutRepo2 = new FileWorkoutRepository(Path.Combine(_tempDir, "workouts.json"));
            var query2 = new WorkoutQueryService(workoutRepo2);
            var total = query2.GetTotalDurationMinutes(user.Id);
            Assert.AreEqual(4.0, total, 0.01);
        }

        [TestMethod]
        public void UserPersistence_SaveAndReload()
        {
            var userRepo1 = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var userService1 = new UserService(userRepo1);
            var u = userService1.Register("SaveMe", 22, 65.0);

            var userRepo2 = new FileUserRepository(Path.Combine(_tempDir, "users.json"));
            var reloaded = userRepo2.GetById(u.Id);
            Assert.IsNotNull(reloaded);
            Assert.AreEqual("SaveMe", reloaded.Name);
        }

        [TestMethod]
        public void QueryService_GetLast30Days_FiltersCorrectly()
        {
            var (service, _, query, user) = BuildServices();
            service.StartWorkout(user.Id, "Recent", WorkoutType.Cardio);
            var recent = query.GetLast30Days(user.Id).ToList();
            Assert.AreEqual(1, recent.Count);
        }
    }
}