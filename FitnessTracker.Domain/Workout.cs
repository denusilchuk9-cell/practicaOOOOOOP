using System;
using System.Collections.Generic;

namespace FitnessTracker.Domain
{
    public class Workout
    {
        public Guid Id { get; private set; }
        public Guid UserId { get; private set; }
        public string Title { get; private set; }
        public WorkoutType Type { get; private set; }
        public DateTime StartedAt { get; private set; }
        public DateTime? CompletedAt { get; private set; }

        private readonly List<Exercise> _exercises = new List<Exercise>();
        public IReadOnlyList<Exercise> Exercises => _exercises.AsReadOnly();

        public Workout(Guid userId, string title, WorkoutType type, DateTime startedAt)
        {
            if (userId == Guid.Empty)
                throw new ArgumentException("UserId cannot be empty.", nameof(userId));
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Title cannot be empty.", nameof(title));

            Id = Guid.NewGuid();
            UserId = userId;
            Title = title;
            Type = type;
            StartedAt = startedAt;
        }

        public void AddExercise(Exercise exercise)
        {
            if (exercise == null) throw new ArgumentNullException(nameof(exercise));
            if (CompletedAt.HasValue) throw new InvalidOperationException("Cannot add exercise to a completed workout.");
            _exercises.Add(exercise);
        }

        public void Complete(DateTime completedAt)
        {
            if (completedAt < StartedAt)
                throw new ArgumentException("Completion time cannot be before start time.");
            CompletedAt = completedAt;
        }

        public bool IsCompleted => CompletedAt.HasValue;

        public int TotalDurationSeconds()
        {
            int total = 0;
            foreach (var e in _exercises)
                total += e.DurationSeconds;
            return total;
        }
    }
}