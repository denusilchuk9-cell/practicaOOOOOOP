using FitnessTracker.Domain;
using System;
using System.Collections.Generic;

namespace FitnessTracker.Application
{
    public interface IUserRepository
    {
        void Save(User user);
        User GetById(Guid id);
        IEnumerable<User> GetAll();
    }
}