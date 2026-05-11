# Test Strategy

## Critical scenarios
1. Workout lifecycle: create → add exercises → complete
2. Persistence: save → reload → verify state identical
3. Business rules: all invariants enforced in constructors and services
4. Error handling: corrupted file, missing file, invalid input

## Hardest parts to test
- Rehydration via reflection (WorkoutRehydrator, ExerciseRehydrator)
- File I/O timing and cleanup in integration tests
- Console input/output (not tested, manual verification only)

## Where mocks are needed vs real integration
- Unit tests: InMemory repositories (no mocks needed, fast)
- Integration tests: real FileWorkoutRepository with temp directory

## Negative scenarios that could break the project
- JSON file deleted between save and load
- Workout completed with no exercises (valid but edge case)
- User with same name registered twice (allowed, different IDs)
- Exercise with zero weight and zero duration (valid for bodyweight)