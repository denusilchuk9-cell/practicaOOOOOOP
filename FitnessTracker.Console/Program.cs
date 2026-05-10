using FitnessTracker.Application;
using FitnessTracker.Domain;
using FitnessTracker.Infrastructure;
using System;

namespace FitnessTracker.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var userRepo = new InMemoryUserRepository();
            var workoutRepo = new InMemoryWorkoutRepository();
            var service = new WorkoutService(workoutRepo, userRepo);

            var user = new User("Denis", 22, 75.0);
            userRepo.Save(user);

            System.Console.WriteLine($"User created: {user.Name} (ID: {user.Id})");

            var workout = service.StartWorkout(user.Id, "Morning Strength", WorkoutType.Strength);
            System.Console.WriteLine($"Workout started: {workout.Title}");

            service.AddExercise(workout.Id, "Bench Press", 4, 10, 80.0, 120);
            service.AddExercise(workout.Id, "Squat", 4, 8, 100.0, 150);
            service.AddExercise(workout.Id, "Pull-up", 3, 12, 0, 90);

            System.Console.WriteLine($"Exercises added: {workout.Exercises.Count}");

            service.CompleteWorkout(workout.Id);
            System.Console.WriteLine($"Workout completed. Duration: {workout.TotalDurationSeconds()}s");

            System.Console.WriteLine("\n--- History ---");
            foreach (var w in service.GetUserHistory(user.Id))
            {
                System.Console.WriteLine($"[{w.Type}] {w.Title} | {w.Exercises.Count} exercises | Completed: {w.IsCompleted}");
            }

            System.Console.ReadKey();
        }
    }
}