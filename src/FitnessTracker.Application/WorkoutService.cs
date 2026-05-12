using FitnessTracker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.Application
{
    public class WorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IUserRepository _userRepository;

        public WorkoutService(IWorkoutRepository workoutRepository, IUserRepository userRepository)
        {
            _workoutRepository = workoutRepository ?? throw new ArgumentNullException(nameof(workoutRepository));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
        }

        public Workout StartWorkout(Guid userId, string title, WorkoutType type)
        {
            var user = _userRepository.GetById(userId);
            if (user == null)
                throw new InvalidOperationException($"User {userId} not found.");

            var workout = WorkoutFactory.Create(userId, title, type);
            _workoutRepository.Save(workout);
            return workout;
        }

        public void AddExercise(Guid workoutId, string name, int sets, int reps, double weightKg, int durationSeconds)
        {
            var workout = _workoutRepository.GetById(workoutId);
            if (workout == null)
                throw new InvalidOperationException($"Workout {workoutId} not found.");

            var exercise = new Exercise(name, sets, reps, weightKg, durationSeconds);
            workout.AddExercise(exercise);
            _workoutRepository.Save(workout);
        }

        public void CompleteWorkout(Guid workoutId)
        {
            var workout = _workoutRepository.GetById(workoutId);
            if (workout == null)
                throw new InvalidOperationException($"Workout {workoutId} not found.");

            workout.Complete(DateTime.UtcNow);
            _workoutRepository.Save(workout);
        }

        public IEnumerable<Workout> GetUserHistory(Guid userId)
        {
            return _workoutRepository.GetByUserId(userId).OrderByDescending(w => w.StartedAt);
        }
    }
}