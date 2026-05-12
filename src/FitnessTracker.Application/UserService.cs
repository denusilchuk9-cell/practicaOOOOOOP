using FitnessTracker.Domain;
using System;

namespace FitnessTracker.Application
{
    public class UserService
    {
        private readonly IUserRepository _repo;

        public UserService(IUserRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public User Register(string name, int age, double weight)
        {
            var user = new User(name, age, weight);
            _repo.Save(user);
            return user;
        }

        public User GetById(Guid id)
        {
            var user = _repo.GetById(id);
            if (user == null)
                throw new InvalidOperationException($"User {id} not found.");
            return user;
        }
    }
}