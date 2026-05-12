# Test Matrix

| Use Case | Unit Tests | Integration Tests |
|---|---|---|
| UC-01: Create workout | StartWorkout_ValidUser_ReturnsWorkout | FullCycle_CreateAddComplete_PersistsCorrectly |
| UC-01: Invalid user | StartWorkout_NonExistentUser_Throws | — |
| UC-02: Add exercise | AddExercise_ToActiveWorkout_IncreasesCount | Persistence_WorkoutSavedAndReloaded |
| UC-02: Add to completed | AddExercise_ToCompletedWorkout_Throws | — |
| UC-03: Complete workout | CompleteWorkout_SetsIsCompleted | FullCycle_CreateAddComplete_PersistsCorrectly |
| UC-03: Complete before start | CompleteWorkout_BeforeStart_Throws | — |
| UC-04: Analytics | GetTotalDurationMinutes_OnlyCompleted | Analytics_AfterReload_CorrectStats |
| UC-04: Longest workout | GetLongestWorkout_ReturnsCorrect | — |
| UC-05: User registration | UserService_Register_SavesUser | UserPersistence_SaveAndReload |
| Domain: User validation | CreateUser_EmptyName_Throws | — |
| Domain: User validation | CreateUser_NegativeAge_Throws | — |
| Domain: User validation | CreateUser_NegativeWeight_Throws | — |
| Domain: Exercise validation | CreateExercise_NegativeSets_Throws | — |
| Search: By type | GetByType_ReturnsCorrectType | — |
| Search: Last 30 days | — | QueryService_GetLast30Days_FiltersCorrectly |
| Search: Group by type | GetWorkoutCountByType_GroupsCorrectly | — |
| Strategy: Strength calc | StrengthCalorieCalculator_ReturnsPosit