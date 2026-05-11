using System;
using System.Collections.Generic;
using System.Linq;
using FitnessTracker.Application;
using FitnessTracker.Domain;

namespace FitnessTracker.Infrastructure
{
    public class FileUserRepository : IUserRepository
    {
        private readonly JsonDataStore<UserDto> _store;
        private readonly Dictionary<Guid, User> _cache = new Dictionary<Guid, User>();
        private bool _loaded = false;

        public FileUserRepository(string filePath)
        {
            _store = new JsonDataStore<UserDto>(filePath);
        }

        public void EnsureLoaded()
        {
            if (_loaded) return;
            var dtos = _store.LoadAsync().GetAwaiter().GetResult();
            foreach (var dto in dtos)
            {
                var user = UserRehydrator.Rehydrate(dto.Id, dto.Name, dto.AgeYears, dto.WeightKg);
                _cache[user.Id] = user;
            }
            _loaded = true;
        }

        public void Save(User user)
        {
            EnsureLoaded();
            _cache[user.Id] = user;
            Persist();
        }

        public User GetById(Guid id)
        {
            EnsureLoaded();
            _cache.TryGetValue(id, out var u);
            return u;
        }

        public IEnumerable<User> GetAll()
        {
            EnsureLoaded();
            return _cache.Values;
        }

        private void Persist()
        {
            var dtos = _cache.Values.Select(u => new UserDto
            {
                Id = u.Id,
                Name = u.Name,
                AgeYears = u.AgeYears,
                WeightKg = u.WeightKg
            }).ToList();
            _store.SaveAsync(dtos).GetAwaiter().GetResult();
        }
    }
}