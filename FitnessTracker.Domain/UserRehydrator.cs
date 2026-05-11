using System;
using System.Reflection;

namespace FitnessTracker.Domain
{
    public static class UserRehydrator
    {
        public static User Rehydrate(Guid id, string name, int ageYears, double weightKg)
        {
            var user = new User(name, ageYears, weightKg);

            var idField = typeof(User).GetField("<Id>k__BackingField",
                BindingFlags.NonPublic | BindingFlags.Instance);
            idField?.SetValue(user, id);

            return user;
        }
    }
}