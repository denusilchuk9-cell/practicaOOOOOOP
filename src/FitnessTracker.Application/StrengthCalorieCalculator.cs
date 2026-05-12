using FitnessTracker.Domain;

namespace FitnessTracker.Application
{
    public class StrengthCalorieCalculator : ICalorieCalculator
    {
        public double Calculate(Workout workout, User user)
        {
            double totalVolume = 0;
            foreach (var e in workout.Exercises)
                totalVolume += e.Sets * e.Reps * e.WeightKg;
            return totalVolume * 0.05;
        }
    }

    public class CardioCalorieCalculator : ICalorieCalculator
    {
        public double Calculate(Workout workout, User user)
        {
            return (workout.TotalDurationSeconds() / 60.0) * (user.WeightKg * 0.1);
        }
    }

    public class DefaultCalorieCalculator : ICalorieCalculator
    {
        public double Calculate(Workout workout, User user)
        {
            return workout.TotalDurationSeconds() * 0.08;
        }
    }
}