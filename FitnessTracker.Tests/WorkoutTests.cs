using FitnessTracker.Application;
using FitnessTracker.Domain;
using FitnessTracker.Infrastructure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace FitnessTracker.Tests
{
    [TestClass]
    public class WorkoutTests
    {
        private IWorkoutRepository _workoutRepo;
        private IUserRepository _userRepo;
        private WorkoutService _service;
        private User _user;

        [TestInitialize]
        public void Setup()
        {
            _workoutRepo = new InMemoryWorkoutRepository();
            _userRepo = new InMemoryUserRepository();
            _service = new WorkoutService(_workoutRepo, _userRepo);
            _user = new User("TestUser", 25, 70.0);
            _userRepo.Save(_user);
        }

        [TestMethod]
        public void StartWorkout_ValidUser_ReturnsWorkout()
        {
            var workout = _service.StartWorkout(_user.Id, "Test Workout", WorkoutType.Cardio);
            Assert.IsNotNull(workout);
            Assert.AreEqual("Test Workout", workout.Title);
        }

        [TestMethod]
        public void AddExercise_ToActiveWorkout_IncreasesCount()
        {
            var workout = _service.StartWorkout(_user.Id, "Strength Day", WorkoutType.Strength);
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
        public void AddExercise_ToCompletedWorkout_ThrowsException()
        {
            var workout = _service.StartWorkout(_user.Id, "Done", WorkoutType.Flexibility);
            _service.CompleteWorkout(workout.Id);
            _service.AddExercise(workout.Id, "Stretch", 1, 1, 0, 60);
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void StartWorkout_NonExistentUser_ThrowsException()
        {
            _service.StartWorkout(Guid.NewGuid(), "Ghost Workout", WorkoutType.Strength);
        }

        [TestMethod]
        public void GetUserHistory_ReturnsOnlyUserWorkouts()
        {
            _service.StartWorkout(_user.Id, "W1", WorkoutType.Cardio);
            _service.StartWorkout(_user.Id, "W2", WorkoutType.Strength);

            var other = new User("OtherUser", 30, 80.0);
            _userRepo.Save(other);
            _service.StartWorkout(other.Id, "W3", WorkoutType.HIIT);

            var history = _service.GetUserHistory(_user.Id);
            int count = 0;
            foreach (var _ in history) count++;
            Assert.AreEqual(2, count);
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
        public void CreateUser_EmptyName_ThrowsException()
        {
            new User("", 25, 70.0);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void CreateExercise_NegativeSets_ThrowsException()
        {
            new Exercise("Squat", -1, 10, 50.0, 60);
        }
    }
}