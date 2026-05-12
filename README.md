# FitnessTracker

Console-based fitness tracking application built with .NET Framework 4.8.1.

## Features
- Create and manage workout sessions
- Add exercises with sets, reps, weight and duration
- Track workout history per user
- Analytics: total time, calories burned, breakdown by type
- Search workouts by type or date range
- Data persisted to JSON files between sessions

## Project Structure
FitnessTracker/
├── FitnessTracker.Domain/        # Entities, interfaces, enums
├── FitnessTracker.Application/   # Services, business rules, Strategy pattern
├── FitnessTracker.Infrastructure/# File repositories, JSON persistence
├── FitnessTracker.Console/       # Interactive console menu
└── FitnessTracker.Tests/         # Unit and integration tests (28 tests)
## How to run
1. Open `FitnessTracker.slnx` in Visual Studio 2022
2. Set `FitnessTracker.Console` as Startup Project
3. Press F5

## How to run tests
`Test → Run All Tests` in Visual Studio

Data is saved to `data/` folder next to the executable.

## Architecture
- **Domain**: pure business logic, no dependencies
- **Application**: orchestrates domain, depends on interfaces
- **Infrastructure**: implements interfaces, handles file I/O
- **Console**: UI only, no business logic

## Patterns used
- Repository Pattern — IWorkoutRepository, IUserRepository
- Factory Pattern — WorkoutFactory
- Strategy Pattern — ICalorieCalculator