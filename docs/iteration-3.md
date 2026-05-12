# Iteration 3 — Lab 36

## Test results
- Unit tests: 20
- Integration tests: 8
- Total: 28 tests, all passing

## Coverage
- Domain layer: ~95% (all invariants tested)
- Application layer: ~85% (all service methods tested)
- Infrastructure layer: ~75% (persistence tested via integration tests)
- Console layer: 0% (UI not unit tested, manual verification)

## Refactoring done
- Separated query logic into WorkoutQueryService (SRP)
- Rehydrators isolate reflection usage to one place
- JsonDataStore<T> is generic and reusable for any entity

## Code smells removed
- Program.cs no longer contains business logic
- No direct file I/O in service layer
- No hardcoded paths in business code

## Risks before Lab 37
- Console input does not validate all edge cases (letters instead of numbers)
- Rehydration via reflection may break if backing field names change in future .NET versions
- No retry policy on file save failure
- Coverage tool not yet connected to CI pipeline

## What needs to be finished in Lab 37
- USER_GUIDE.md
- DEVELOPER_GUIDE.md
- CHANGELOG.md
- FINAL_REPORT.md
- DEMO.md
- syllabus-coverage.md