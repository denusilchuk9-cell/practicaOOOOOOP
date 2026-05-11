using FitnessTracker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.Application
{
    public class WorkoutQueryService
    {
        private readonly IWorkoutRepository _repo;

        public WorkoutQueryService(IWorkoutRepository repo)
        {
            _repo = repo;
        }

        public IEnumerable<Workout> GetCompletedByUser(Guid userId)
        {
            return _repo.GetByUserId(userId).Where(w => w.IsCompleted);
        }

        public IEnumerable<Workout> GetByType(Guid userId, WorkoutType type)
        {
            return _repo.GetByUserId(userId).Where(w => w.Type == type);
        }

        public IEnumerable<Workout> GetLast30Days(Guid userId)
        {
            var cutoff = DateTime.UtcNow.AddDays(-30);
            return _repo.GetByUserId(userId).Where(w => w.StartedAt >= cutoff)
                        .OrderByDescending(w => w.StartedAt);
        }

        public Dictionary<WorkoutType, int> GetWorkoutCountByType(Guid userId)
        {
            return _repo.GetByUserId(userId)
                        .GroupBy(w => w.Type)
                        .ToDictionary(g => g.Key, g => g.Count());
        }

        public double GetTotalDurationMinutes(Guid userId)
        {
            return _repo.GetByUserId(userId)
                        .Where(w => w.IsCompleted)
                        .Sum(w => w.TotalDurationSeconds()) / 60.0;
        }

        public Workout GetLongestWorkout(Guid userId)
        {
            return _repo.GetByUserId(userId)
                        .Where(w => w.IsCompleted)
                        .OrderByDescending(w => w.TotalDurationSeconds())
                        .FirstOrDefault();
        }
    }
}