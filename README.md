# FitnessTracker

Консольний застосунок для відстеження тренувань на .NET Framework 4.8.1.

## Можливості
- Створення та керування тренуваннями
- Додавання вправ із підходами, повтореннями, вагою та тривалістю
- Перегляд історії тренувань
- Аналітика: загальний час, спалені калорії, розбивка по типах
- Пошук тренувань за типом або датою
- Дані зберігаються у JSON файлах між сесіями

## Структура проєкту
FitnessTracker/
├── src/
│   ├── FitnessTracker.Domain/         # Сутності, інтерфейси, enum
│   ├── FitnessTracker.Application/    # Сервіси, бізнес-логіка, патерн Strategy
│   ├── FitnessTracker.Infrastructure/ # Репозиторії, JSON persistence
│   └── FitnessTracker.Console/        # Інтерактивне консольне меню
├── tests/
│   └── FitnessTracker.Tests/          # Юніт та інтеграційні тести (28 тестів)
├── docs/                              # Документація проєкту
├── .github/workflows/                 # CI pipeline
├── README.md
└── TESTING.md
