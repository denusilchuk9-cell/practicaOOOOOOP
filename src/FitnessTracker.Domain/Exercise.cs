using System;

namespace FitnessTracker.Domain
{
    public class Exercise
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int Sets { get; private set; }
        public int Reps { get; private set; }
        public double WeightKg { get; private set; }
        public int DurationSeconds { get; private set; }

        public Exercise(string name, int sets, int reps, double weightKg, int durationSeconds)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Exercise name cannot be empty.", nameof(name));
            if (sets < 0) throw new ArgumentException("Sets cannot be negative.", nameof(sets));
            if (reps < 0) throw new ArgumentException("Reps cannot be negative.", nameof(reps));
            if (weightKg < 0) throw new ArgumentException("Weight cannot be negative.", nameof(weightKg));
            if (durationSeconds < 0) throw new ArgumentException("Duration cannot be negative.", nameof(durationSeconds));

            Id = Guid.NewGuid();
            Name = name;
            Sets = sets;
            Reps = reps;
            WeightKg = weightKg;
            DurationSeconds = durationSeconds;
        }
    }
}