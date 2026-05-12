namespace FitnessTracker.Domain
{
    public interface ICalorieCalculator
    {
        double Calculate(Workout workout, User user);
    }
}