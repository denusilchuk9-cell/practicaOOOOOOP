# Iteration 1 — Lab 34

## What works
- Domain model: User, Workout, Exercise, WorkoutType
- WorkoutService with business rules
- InMemory repositories
- Console demo (vertical slice)
- 8 unit tests pass

## Artifacts in repository
- docs/vision.md
- docs/backlog.md
- src: Domain, Application, Infrastructure, Console, Tests
- FitnessTracker.slnx

## Scenarios for Iteration 2
1. File persistence (save/load workouts and users)
2. Extended console menu with all use cases
3. LINQ analytics and search

## Risks
- Rehydration of domain objects from JSON without breaking encapsulation
- Console input validation edge cases

## Classes prepared for extension
- IWorkoutRepository — ready for file implementation
- IUserRepository — ready for file implementation
- ICalorieCalculator — interface ready for Strategy pattern