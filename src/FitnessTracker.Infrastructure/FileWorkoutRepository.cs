using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using FitnessTracker.Application;
using FitnessTracker.Domain;

namespace FitnessTracker.Infrastructure
{
    public class FileWorkoutRepository : IWorkoutRepository
    {
        private readonly JsonDataStore<WorkoutDto> _store;
        private readonly SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);
        private Dictionary<Guid, Workout> _cache = new Dictionary<Guid, Workout>();
        private bool _loaded = false;

        public FileWorkoutRepository(string filePath)
        {
            _store = new JsonDataStore<WorkoutDto>(filePath);
        }

        private async Task EnsureLoadedAsync()
        {
            if (_loaded) return;

            await _semaphore.WaitAsync();
            try
            {
                if (_loaded) return;

                var dtos = await _store.LoadAsync();
                var newCache = new Dictionary<Guid, Workout>();
                foreach (var dto in dtos)
                {
                    var workout = MapFromDto(dto);
                    newCache[workout.Id] = workout;
                }
                _cache = newCache;
                _loaded = true;
            }
            finally
            {
                _semaphore.Release();
            }
        }

        private async Task PersistAsync()
        {
            await _semaphore.WaitAsync();
            try
            {
                var dtos = _cache.Values.Select(MapToDto).ToList();
                await _store.SaveAsync(dtos);
            }
            finally
            {
                _semaphore.Release();
            }
        }

        public void Save(Workout workout)
        {
            if (workout == null) throw new ArgumentNullException(nameof(workout));
            EnsureLoadedAsync().GetAwaiter().GetResult();
            _cache[workout.Id] = workout;
            PersistAsync().GetAwaiter().GetResult();
        }

        public Workout GetById(Guid id)
        {
            EnsureLoadedAsync().GetAwaiter().GetResult();
            _cache.TryGetValue(id, out var workout);
            return workout;
        }

        public IEnumerable<Workout> GetByUserId(Guid userId)
        {
            EnsureLoadedAsync().GetAwaiter().GetResult();
            return _cache.Values.Where(w => w.UserId == userId);
        }

        public IEnumerable<Workout> GetAll()
        {
            EnsureLoadedAsync().GetAwaiter().GetResult();
            return _cache.Values;
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
            {
                var exercise = ExerciseRehydrator.Rehydrate(
                    e.Id, e.Name, e.Sets, e.Reps, e.WeightKg, e.DurationSeconds);
                workout.AddExerciseForRehydration(exercise);
            }
            return workout;
        }