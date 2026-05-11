using System;
using System.Reflection;

namespace FitnessTracker.Domain
{
    public static class ExerciseRehydrator
    {
        public static Exercise Rehydrate(Guid id, string name, int sets,
            int reps, double weightKg, int durationSeconds)
        {
            var exercise = new Exercise(name, sets, reps, weightKg, durationSeconds);

            var idField = typeof(Exercise).GetField("<Id>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(exercise, id);

            return exercise;
        }
    }
}