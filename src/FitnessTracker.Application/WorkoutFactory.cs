using FitnessTracker.Domain;
using System;

namespace FitnessTracker.Application
{
    public static class WorkoutFactory
    {
        public static Workout Create(Guid userId, string title, WorkoutType type)
        {
            return new Workout(userId, title, type, DateTime.UtcNow);
        }
    }
}