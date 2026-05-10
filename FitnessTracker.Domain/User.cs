using System;

namespace FitnessTracker.Domain
{
    public class User
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public int AgeYears { get; private set; }
        public double WeightKg { get; private set; }

        public User(string name, int ageYears, double weightKg)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));
            if (ageYears <= 0)
                throw new ArgumentException("Age must be positive.", nameof(ageYears));
            if (weightKg <= 0)
                throw new ArgumentException("Weight must be positive.", nameof(weightKg));

            Id = Guid.NewGuid();
            Name = name;
            AgeYears = ageYears;
            WeightKg = weightKg;
        }
    }
}