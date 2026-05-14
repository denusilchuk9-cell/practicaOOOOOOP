# Iteration 2 — Lab 35

## Completed use cases
1. UC-01: Create and complete a workout with exercises
2. UC-02: View full workout history
3. UC-03: Analytics — total time, calories, breakdown by type
4. UC-04: Search workouts by type and last 30 days
5. UC-05: Select or create user profile on startup
6. UC-06: Export data to XML

## Business rules implemented
1. Cannot add exercise to a completed workout
2. Workout completion time cannot be before start time
3. User must exist before starting a workout
4. Exercise sets, reps, weight, duration cannot be negative
5. User name cannot be empty, age and weight must be positive

## Pattern used: Strategy
- ICalorieCalculator interface with three implementations
- Strength: volume-based (sets × reps × weight × 0.05)
- Cardio: duration and weight based
- Default: duration-based fallback

## LINQ queries
1. Filter completed workouts by user
2. Filter by workout type
3. Filter last 30 days ordered by date
4. Group by type with count (Dictionary)
5. Sum total duration for completed workouts
6. Find longest workout by duration

## Specialized collection used
- Dictionary for cache in repositories
- Dictionary<WorkoutType, int> for workout count by type

## Classes changed from Iteration 1
- Program.cs — full interactive menu with XML export
- Added: JsonDataStore<T>, WorkoutDto, FileWorkoutRepository, FileUserRepository
- Added: WorkoutRehydrator, ExerciseRehydrator, UserRehydrator
- Added: WorkoutQueryService, UserService
- Added: StrengthCalorieCalculator, CardioCalorieCalculator, DefaultCalorieCalculator
- Added: WorkoutXmlExporter, RetryHelper

## Riskiest scenarios for integration testing
- Save workout → restart app → reload and verify state
- Corrupted JSON file on startup
- Missing data directory on first run

## What integration tests must verify in Lab 36
- Full save/reload cycle preserves all workout data
- Exercises are reloaded correctly after restart
- Corrupted file does not crash application
- Analytics return correct values after reload