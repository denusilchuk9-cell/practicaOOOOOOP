# Testing Guide

## How to run tests
Open Visual Studio → `Test → Run All Tests`

Or via command line:
dotnet test
## Test structure
All tests are in `FitnessTracker.Tests/WorkoutTests.cs`

### Unit tests (20)
- Domain invariants: User, Exercise, Workout constructors
- WorkoutService: all business operations
- WorkoutQueryService: all LINQ queries
- Strategy pattern: all three calorie calculators
- Negative scenarios: exceptions on invalid input

### Integration tests (8)
- Persistence_WorkoutSavedAndReloaded
- Persistence_MultipleWorkouts_AllReloaded
- Persistence_MissingFile_ReturnsEmpty
- Persistence_CorruptedFile_ReturnsEmpty
- FullCycle_CreateAddComplete_PersistsCorrectly
- Analytics_AfterReload_CorrectStats
- UserPersistence_SaveAndReload
- QueryService_GetLast30Days_FiltersCorrectly

## Negative scenarios covered
- Empty user name → ArgumentException
- Negative age/weight → ArgumentException
- Negative exercise sets → ArgumentException
- Add exercise to completed workout → InvalidOperationException
- Start workout for nonexistent user → InvalidOperationException
- Complete workout before start time → ArgumentException
- Corrupted JSON file → empty collection returned, no crash
- Missing JSON file → empty collection returned, no crash

## Test data
Integration tests use temporary directories created per test and deleted after.
No shared state between tests.