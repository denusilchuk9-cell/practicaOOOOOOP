using System;
using System.Reflection;
using System.Collections.Generic;

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
            {
                var completedAtField = typeof(Workout).GetField("<CompletedAt>k__BackingField",
                    BindingFlags.NonPublic | BindingFlags.Instance);
                completedAtField?.SetValue(workout, completedAt);
            }

            return workout;
        }
    }
}