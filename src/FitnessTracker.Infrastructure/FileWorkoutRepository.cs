using System;
using System.Collections.Generic;
using System.Linq;
using FitnessTracker.Application;
using FitnessTracker.Domain;

namespace FitnessTracker.Infrastructure
{
    public class FileWorkoutRepository : IWorkoutRepository
    {
        private readonly JsonDataStore<WorkoutDto> _store;
        private readonly Dictionary<Guid, Workout> _cache = new Dictionary<Guid, Workout>();
        private bool _loaded = false;

        public FileWorkoutRepository(string filePath)
        {
            _store = new JsonDataStore<WorkoutDto>(filePath);
        }

        public void EnsureLoaded()
        {
            if (_loaded) return;
            var dtos = _store.LoadAsync().GetAwaiter().GetResult();
            foreach (var dto in dtos)
            {
                var workout = MapFromDto(dto);
                _cache[workout.Id] = workout;
            }
            _loaded = true;
        }

        public void Save(Workout workout)
        {
            EnsureLoaded();
            _cache[workout.Id] = workout;
            Persist();
        }

        public Workout GetById(Guid id)
        {
            EnsureLoaded();
            _cache.TryGetValue(id, out var w);
            return w;
        }

        public IEnumerable<Workout> GetByUserId(Guid userId)
        {
            EnsureLoaded();
            return _cache.Values.Where(w => w.UserId == userId);
        }

        public IEnumerable<Workout> GetAll()
        {
            EnsureLoaded();
            return _cache.Values;
        }

        private void Persist()
        {
            var dtos = _cache.Values.Select(MapToDto).ToList();
            _store.SaveAsync(dtos).GetAwaiter().GetResult();
        }

        private WorkoutDto MapToDto(Workout w)
        {
            var dto = new WorkoutDto
            {
                Id = w.Id,
                UserId = w.UserId,
                Title = w.Title,
                Type = (int)w.Type,
                StartedAt = w.StartedAt,
                CompletedAt = w.CompletedAt
            };
            foreach (var e in w.Exercises)
                dto.Exercises.Add(new ExerciseDto
                {
                    Id = e.Id,
                    Name = e.Name,
                    Sets = e.Sets,
                    Reps = e.Reps,
                    WeightKg = e.WeightKg,
                    DurationSeconds = e.DurationSeconds
                });
            return dto;
        }

        private Workout MapFromDto(WorkoutDto dto)
        {
            var workout = WorkoutRehydrator.Rehydrate(
                dto.Id, dto.UserId, dto.Title,
                (WorkoutType)dto.Type, dto.StartedAt, dto.CompletedAt);
            foreach (var e in dto.Exercises)
                workout.AddExercise(ExerciseRehydrator.Rehydrate(
                    e.Id, e.Name, e.Sets, e.Reps, e.WeightKg, e.DurationSeconds));
            return workout;
        }
    }
}