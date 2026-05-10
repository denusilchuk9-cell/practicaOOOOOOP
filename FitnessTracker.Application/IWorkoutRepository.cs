using FitnessTracker.Domain;
using System;
using System.Collections.Generic;

namespace FitnessTracker.Application
{
    public interface IWorkoutRepository
    {
        void Save(Workout workout);
        Workout GetById(Guid id);
        IEnumerable<Workout> GetByUserId(Guid userId);
        IEnumerable<Workout> GetAll();
    }
}