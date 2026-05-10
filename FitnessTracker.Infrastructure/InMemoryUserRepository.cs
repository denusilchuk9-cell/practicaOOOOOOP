using FitnessTracker.Application;
using FitnessTracker.Domain;
using System;
using System.Collections.Generic;

namespace FitnessTracker.Infrastructure
{
    public class InMemoryUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _store = new Dictionary<Guid, User>();

        public void Save(User user)
        {
            if (user == null) throw new ArgumentNullException(nameof(user));
            _store[user.Id] = user;
        }

        public User GetById(Guid id)
        {
            _store.TryGetValue(id, out var user);
            return user;
        }

        public IEnumerable<User> GetAll()
        {
            return _store.Values;
        }
    }
}