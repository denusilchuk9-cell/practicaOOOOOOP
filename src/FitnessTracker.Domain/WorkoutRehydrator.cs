using System;
using System.Reflection;

namespace FitnessTracker.Domain
{
    public static class WorkoutRehydrator
    {
        public static Workout Rehydrate(Guid id, Guid userId, string title,
            WorkoutType type, DateTime startedAt, DateTime? completedAt)
        {
            var workout = new Workout(userId, title, type, startedAt);

            var idField = typeof(Workout).GetField("<Id>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(workout, id);

            if (completedAt.HasValue)
                workout.Complete(completedAt.Value);

            return workout;
        }
    }
}