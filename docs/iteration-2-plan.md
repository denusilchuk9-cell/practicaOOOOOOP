# Iteration 2 Plan

## Scenarios moving to implementation
- UC-01: Start workout (already done, extending)
- UC-02: Add exercise to workout
- UC-03: View workout history
- UC-04: Analytics (total time, calories, by type)
- UC-05: Search by type and date

## Classes unchanged from Iteration 1
- User, Workout, Exercise, WorkoutType
- WorkoutFactory
- InMemoryWorkoutRepository, InMemoryUserRepository

## Extension points used
- ICalorieCalculator → StrengthCalorieCalculator, CardioCalorieCalculator, DefaultCalorieCalculator
- IWorkoutRepository → FileWorkoutRepository
- IUserRepository → FileUserRepository

## Visible risks
- DataContractJsonSerializer requires System.Runtime.Serialization assembly reference
- Rehydration uses reflection — fragile if property names change
- Console input parsing does not handle all edge cases