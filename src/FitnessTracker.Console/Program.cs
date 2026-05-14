using FitnessTracker.Application;
using FitnessTracker.Domain;
using FitnessTracker.Infrastructure;
using System;
using System.IO;
using System.Linq;

namespace FitnessTracker.Console
{
    class Program
    {
        static IWorkoutRepository _workoutRepo;
        static IUserRepository _userRepo;
        static WorkoutService _workoutService;
        static UserService _userService;
        static WorkoutQueryService _queryService;
        static User _currentUser;

        static void Main(string[] args)
        {
            var dataDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data");
            _workoutRepo = new FileWorkoutRepository(Path.Combine(dataDir, "workouts.json"));
            _userRepo = new FileUserRepository(Path.Combine(dataDir, "users.json"));
            _workoutService = new WorkoutService(_workoutRepo, _userRepo);
            _userService = new UserService(_userRepo);
            _queryService = new WorkoutQueryService(_workoutRepo);

            System.Console.WriteLine("=== FitnessTracker ===");
            SelectOrCreateUser();
            MainMenu();
        }

        static void SelectOrCreateUser()
        {
            var users = _userRepo.GetAll().ToList();
            if (users.Count == 0)
            {
                System.Console.WriteLine("No users found. Create a new profile.");
                _currentUser = CreateUser();
                return;
            }

            System.Console.WriteLine("\nExisting users:");
            for (int i = 0; i < users.Count; i++)
                System.Console.WriteLine($"  {i + 1}. {users[i].Name} (age {users[i].AgeYears}, {users[i].WeightKg}kg)");
            System.Console.WriteLine($"  {users.Count + 1}. Create new user");
            System.Console.Write("Select: ");

            if (int.TryParse(System.Console.ReadLine(), out int choice) && choice <= users.Count && choice >= 1)
                _currentUser = users[choice - 1];
            else
                _currentUser = CreateUser();

            System.Console.WriteLine($"\nLogged in as: {_currentUser.Name}");
        }

        static User CreateUser()
        {
            System.Console.Write("Name: ");
            var name = System.Console.ReadLine();
            System.Console.Write("Age: ");
            int.TryParse(System.Console.ReadLine(), out int age);
            System.Console.Write("Weight (kg): ");
            double.TryParse(System.Console.ReadLine(), out double weight);
            return _userService.Register(name, age, weight);
        }

        static void MainMenu()
        {
            while (true)
            {
                System.Console.WriteLine($"\n--- Main Menu [{_currentUser.Name}] ---");
                System.Console.WriteLine("1. Start new workout");
                System.Console.WriteLine("2. View workout history");
                System.Console.WriteLine("3. Analytics");
                System.Console.WriteLine("4. Search workouts");
                System.Console.WriteLine("5. Export data to XML");
                System.Console.WriteLine("0. Exit");
                System.Console.Write("Choice: ");

                switch (System.Console.ReadLine())
                {
                    case "1": StartWorkoutMenu(); break;
                    case "2": ViewHistory(); break;
                    case "3": AnalyticsMenu(); break;
                    case "4": SearchMenu(); break;
                    case "5": ExportMenu(); break;
                    case "0": return;
                    default: System.Console.WriteLine("Invalid choice."); break;
                }
            }
        }

        static void StartWorkoutMenu()
        {
            System.Console.Write("Workout title: ");
            var title = System.Console.ReadLine();
            System.Console.WriteLine("Type: 1=Strength, 2=Cardio, 3=Flexibility, 4=HIIT");
            System.Console.Write("Choice: ");
            int.TryParse(System.Console.ReadLine(), out int typeChoice);
            var type = (WorkoutType)(typeChoice - 1);

            var workout = _workoutService.StartWorkout(_currentUser.Id, title, type);
            System.Console.WriteLine($"Workout '{workout.Title}' started.");

            while (true)
            {
                System.Console.WriteLine("\n  1. Add exercise");
                System.Console.WriteLine("  2. Complete workout");
                System.Console.WriteLine("  0. Back (workout stays active)");
                System.Console.Write("  Choice: ");

                switch (System.Console.ReadLine())
                {
                    case "1": AddExerciseMenu(workout); break;
                    case "2":
                        _workoutService.CompleteWorkout(workout.Id);
                        var calc = GetCalculator(workout.Type);
                        var cal = calc.Calculate(workout, _currentUser);
                        System.Console.WriteLine($"Workout completed! Estimated calories burned: {cal:F1}");
                        return;
                    case "0": return;
                }
            }
        }

        static void AddExerciseMenu(Workout workout)
        {
            System.Console.Write("Exercise name: ");
            var name = System.Console.ReadLine();
            System.Console.Write("Sets: ");
            int.TryParse(System.Console.ReadLine(), out int sets);
            System.Console.Write("Reps: ");
            int.TryParse(System.Console.ReadLine(), out int reps);
            System.Console.Write("Weight (kg, 0 if bodyweight): ");
            double.TryParse(System.Console.ReadLine(), out double weight);
            System.Console.Write("Duration (seconds): ");
            int.TryParse(System.Console.ReadLine(), out int duration);

            _workoutService.AddExercise(workout.Id, name, sets, reps, weight, duration);
            System.Console.WriteLine($"Exercise '{name}' added.");
        }

        static void ViewHistory()
        {
            var history = _workoutService.GetUserHistory(_currentUser.Id).ToList();
            if (history.Count == 0) { System.Console.WriteLine("No workouts yet."); return; }

            System.Console.WriteLine($"\n--- Workout History ({history.Count} total) ---");
            foreach (var w in history)
            {
                var status = w.IsCompleted ? "Done" : "Active";
                System.Console.WriteLine($"[{w.Type}] {w.Title} | {w.Exercises.Count} exercises | {w.StartedAt:dd.MM.yyyy} | {status}");
            }
        }

        static void AnalyticsMenu()
        {
            System.Console.WriteLine("\n--- Analytics ---");
            var totalMin = _queryService.GetTotalDurationMinutes(_currentUser.Id);
            System.Console.WriteLine($"Total training time: {totalMin:F1} min");

            var byType = _queryService.GetWorkoutCountByType(_currentUser.Id);
            System.Console.WriteLine("Workouts by type:");
            foreach (var kv in byType)
                System.Console.WriteLine($"  {kv.Key}: {kv.Value}");

            var longest = _queryService.GetLongestWorkout(_currentUser.Id);
            if (longest != null)
                System.Console.WriteLine($"Longest workout: {longest.Title} ({longest.TotalDurationSeconds()}s)");
        }

        static void SearchMenu()
        {
            System.Console.WriteLine("\n--- Search ---");
            System.Console.WriteLine("1. By type");
            System.Console.WriteLine("2. Last 30 days");
            System.Console.Write("Choice: ");

            switch (System.Console.ReadLine())
            {
                case "1":
                    System.Console.WriteLine("Type: 1=Strength, 2=Cardio, 3=Flexibility, 4=HIIT");
                    System.Console.Write("Choice: ");
                    int.TryParse(System.Console.ReadLine(), out int t);
                    var byType = _queryService.GetByType(_currentUser.Id, (WorkoutType)(t - 1));
                    foreach (var w in byType)
                        System.Console.WriteLine($"  {w.Title} | {w.StartedAt:dd.MM.yyyy}");
                    break;
                case "2":
                    var recent = _queryService.GetLast30Days(_currentUser.Id);
                    foreach (var w in recent)
                        System.Console.WriteLine($"  {w.Title} | {w.StartedAt:dd.MM.yyyy}");
                    break;
            }
        }

        static void ExportMenu()
        {
            System.Console.WriteLine("\n--- Export ---");
            System.Console.WriteLine("1. Export all workouts to XML");
            System.Console.Write("Choice: ");

            if (System.Console.ReadLine() == "1")
            {
                var exportPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "export", $"workouts_{DateTime.Now:yyyyMMdd_HHmmss}.xml");
                var exporter = new WorkoutXmlExporter(_workoutRepo);
                exporter.ExportToXml(exportPath);
                System.Console.WriteLine($"Exported to: {exportPath}");
            }
        }

        static ICalorieCalculator GetCalculator(WorkoutType type)
        {
            switch (type)
            {
                case WorkoutType.Strength: return new StrengthCalorieCalculator();
                case WorkoutType.Cardio: return new CardioCalorieCalculator();
                default: return new DefaultCalorieCalculator();
            }
        }
    }
}