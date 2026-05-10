using FitnessTracker.Application;
using FitnessTracker.Domain;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FitnessTracker.Infrastructure
{
    public class InMemoryWorkoutRepository : IWorkoutRepository
    {
        private readonly Dictionary<Guid, Workout> _store = new Dictionary<Guid, Workout>();

        public void Save(Workout workout)
        {
            if (workout == null) throw new ArgumentNullException(nameof(workout));
            _store[workout.Id] = workout;
        }

        public Workout GetById(Guid id)
        {
            _store.TryGetValue(id, out var workout);
            return workout;
        }

        public IEnumerable<Workout> GetByUserId(Guid userId)
        {
            return _store.Values.Where(w => w.UserId == userId);
        }

        public IEnumerable<Workout> GetAll()
        {
            return _store.Values;
        }
    }
}