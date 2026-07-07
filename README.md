# Minecraft Skins 🎮

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4)](https://dotnet.microsoft.com/)
[![PostgreSQL](https://img.shields.io/badge/PostgreSQL-15-336791)](https://www.postgresql.org/)
[![Docker](https://img.shields.io/badge/Docker-✓-2496ED)](https://www.docker.com/)
[![Tests](https://img.shields.io/badge/Tests-✓-success)](https://github.com/fairwix/MinecraftSkins)

Магазин Minecraft скинов с динамическим ценообразованием на основе курса BTC/USD. 
Цена рассчитывается в реальном времени при просмотре каталога и фиксируется в момент покупки.

## Оглавление
- [Возможности](#-возможности)
- [Технологический стек](#-технологический-стек)
- [Архитектура](#-архитектура)
- [Формула расчёта цены](#-формула-расчёта-цены)
- [Запуск проекта](#-запуск-проекта)
- [API Endpoints](#-api-endpoints)
- [Тестирование](#-тестирование)
- [Мониторинг](#-мониторинг)
- [Безопасность и конкурентность](#-безопасность-и-конкурентность)
- [Особенности реализации](#-особенности-реализации)

## Возможности

-  **Каталог скинов** — просмотр с актуальными ценами
-  **BTC-ценообразование** — автоматический расчёт цены от курса Bitcoin
-  **Покупка** — с записью истории и фиксацией курса
-  **История покупок** — детальная информация о каждой транзакции
-  **Кэширование курса** — с fallback-механизмом для отказоустойчивости
-  **Идемпотентность** — защита от повторных покупок
-  **Soft delete** — безопасное удаление скинов

## Технологический стек

### Backend
- **.NET 8** + ASP.NET Core Web API
- **Entity Framework Core** + PostgreSQL
- **FluentValidation** — валидация DTO
- **AutoMapper** — маппинг сущностей
- **IMemoryCache** — кэширование курса BTC
- **CancellationToken** — передаётся во все асинхронные методы (БД, HTTP-вызовы)
- **ILogger<T>** — структурированное логирование во всех сервисах и middleware

### Frontend
- **HTML5**, **CSS3**, **JavaScript** (vanilla)
- **Bootstrap 5** — адаптивная сетка
- **Font Awesome 6** — иконки

### Инфраструктура
- **Docker** + **Docker Compose**
- **PostgreSQL 15** — база данных
- **Nginx** — раздача статики фронтенда
- **Health Checks** — мониторинг сервисов

### Тестирование
- **xUnit** — фреймворк тестирования
- **Moq** — мокирование зависимостей
- **FluentAssertions** — читаемые assertions
- **ReportGenerator** — отчёты о покрытии

##  Архитектура

Проект разделён на 4 слоя в соответствии с принципами чистой архитектуры:

```
MinecraftSkins/
├── MinecraftSkins.Domain/          # Сущности и бизнес-правила
├── MinecraftSkins.Application/      # Use cases, DTO, интерфейсы
├── MinecraftSkins.Infrastructure/   # EF Core, внешние API, кэш
├── MinecraftSkins.WebAPI/           # API, контроллеры, middleware
├── MinecraftSkins.Tests/            # Unit-тесты
└── frontend/                        # Веб-интерфейс
```

### Слои подробно:

**Domain** — ядро приложения:
- `Skin` — сущность скина (Id, Name, BasePriceUsd, IsAvailable)
- `Purchase` — чек покупки (Id, SkinId, PriceUsdFinal, BtcUsdRate, RateSource)

**Application** — бизнес-логика:
- Интерфейсы репозиториев и сервисов
- DTO и маппинги
- Валидация через FluentValidation
- Калькуляторы цены (Standard/Promo)

**Infrastructure** — детали реализации:
- EF Core DbContext и конфигурации
- Реализация репозиториев
- HTTP-клиент для CoinGecko API
- Кэширование и миграции

**WebAPI** — точка входа:
- Контроллеры (тонкие)
- Middleware для обработки ошибок
- Swagger документация
- Health Checks

##  Формула расчёта цены

```
FinalPrice = BasePriceUsd × (1 + BTC_USD_Rate ÷ 50000)
```

- Цена округляется до **2 знаков** после запятой
- При BTC = $50,000 цена увеличивается в 2 раза
- При BTC = $0 цена равна базовой

### Реализованы два калькулятора:
- **StandardPriceCalculator** — базовая формула
- **PromoPriceCalculator** — со скидкой 10% (демонстрация расширяемости)

##  Запуск проекта

### Через Docker (рекомендуется)

```bash
# Клонировать репозиторий
git clone https://github.com/fairwix/MinecraftSkins.git
cd MinecraftSkins

# Запустить все сервисы
docker-compose up --build
```

**После запуска:**
- Frontend: http://localhost:8080
- Backend API: http://localhost:5001
- Swagger UI: http://localhost:5001/swagger
- Health Check: http://localhost:5001/health

**Остановка:**
```bash
docker-compose down
```

### Локальный запуск (без Docker)

**Требования:**
- .NET 8 SDK
- PostgreSQL 15
- Любой браузер для фронтенда

```bash
# 1. Настройте строку подключения в appsettings.Development.json
# {
#   "ConnectionStrings": {
#     "DefaultConnection": "Host=localhost;Port=5432;Database=minecraftskins;Username=postgres;Password=postgres"
#   }
# }

# 2. Примените миграции
dotnet ef database update -p MinecraftSkins.Infrastructure -s MinecraftSkins.WebAPI

# 3. Запустите backend
cd MinecraftSkins.WebAPI
dotnet run
# Backend будет доступен на http://localhost:5131

# 4. В новом терминале запустите фронтенд
cd ../frontend
# Вариант 1: открыть файл в браузере
open index.html
# Вариант 2: через Python
python3 -m http.server 8080
# Frontend будет доступен на http://localhost:8080
```

##  API Endpoints

### Skins
| Метод | Endpoint | Описание | Параметры |
|--------|----------|----------|-----------|
| GET | `/api/skins` | Список скинов | `availableOnly`, `search`, `skip`, `take` |
| GET | `/api/skins/{id}` | Детали скина | — |
| POST | `/api/skins` | Создать скин | `{ name, basePriceUsd, isAvailable }` |
| PUT | `/api/skins/{id}` | Обновить скин | `{ name, basePriceUsd, isAvailable }` |
| DELETE | `/api/skins/{id}` | Удалить скин (soft delete) | — |

### Rates
| Метод | Endpoint | Описание | Ответ |
|--------|----------|----------|-------|
| GET | `/api/rates/btc-usd` | Текущий курс BTC/USD | `{ rate, asOfUtc, source, ageSeconds }` |

### Purchases
| Метод | Endpoint | Описание | Заголовки |
|--------|----------|----------|-----------|
| POST | `/api/purchases` | Купить скин | `X-User-Id`, `Idempotency-Key` |
| GET | `/api/purchases` | История покупок | `mineOnly`, `skinId`, `from`, `to`, `skip`, `take` |
| GET | `/api/purchases/{id}` | Детали покупки | — |

##  Тестирование

Фокус тестирования сделан на **бизнес-логике (Application + Domain)**, так как именно здесь сосредоточена основная сложность и ценность приложения.

###  Что тестируется

**Domain Layer (100%):**
- Сущности `Skin` и `Purchase` — инициализация, свойства

**Application Layer (92%):**
-  **IPriceCalculator** — Standard и Promo стратегии с различными входными данными
-  **PurchaseService** — создание покупки, идемпотентность, optimistic concurrency
-  **SkinService** — CRUD операции, soft delete, фильтрация
-  **FluentValidation** — все DTO (Create/Update Skin, Create Purchase)
-  **AutoMapper** — корректность маппингов
-  **Кастомные исключения** — SkinUnavailableException, ExternalServiceUnavailableException

**Infrastructure Layer (выборочно):**
-  **BtcRateService** — кэширование, fallback при ошибках API, обработка таймаутов
-  **DbContext конфигурации** — настройки сущностей, индексы, глобальные фильтры
-  Миграции и SeedData не тестируются (инфраструктурный код)

**WebAPI Layer (выборочно):**
-  **ExceptionHandlingMiddleware** — обработка всех типов исключений (400, 404, 409, 503)
-  **HealthChecks** — DatabaseHealthCheck, BtcRateHealthCheck
-  Контроллеры тестируются минимально — они содержат только оркестрацию и не имеют бизнес-логики

### Почему такой подход?

> **"Контроллеры должны быть тонкими"** — это один из ключевых принципов чистой архитектуры. 
> 
> Вся бизнес-логика вынесена в Application и Domain слои, которые имеют высокое покрытие тестами. 
> Контроллеры только принимают запросы, вызывают сервисы и возвращают ответы — такая логика 
> тривиальна и не требует обширного тестирования.

### 🔍 Примеры тестов

```csharp
// Тест калькулятора цены
[Theory]
[InlineData(10, 50000, 20)]
[InlineData(20, 25000, 30)]
public void CalculateFinalPrice_ShouldReturnExpected(decimal basePrice, decimal btcRate, decimal expected)
{
    var result = _calculator.CalculateFinalPrice(basePrice, btcRate);
    result.Should().Be(expected);
}

// Тест идемпотентности покупки
[Fact]
public async Task CreatePurchaseAsync_WithSameIdempotencyKey_ReturnsExistingPurchase()
{
    // Arrange
    var key = "test-key";
    var firstPurchase = await _service.CreatePurchaseAsync(skinId, userId, btcRate, key);
    
    // Act
    var secondPurchase = await _service.CreatePurchaseAsync(skinId, userId, btcRate, key);
    
    // Assert
    secondPurchase.Id.Should().Be(firstPurchase.Id);
}
```

###  Запуск тестов

```bash
# Запустить все тесты
dotnet test

# Запустить тесты с детальным выводом
dotnet test -v normal

# Запустить тесты конкретного проекта
dotnet test MinecraftSkins.Tests

# Запустить тесты с покрытием
dotnet test --collect:"XPlat Code Coverage"

# Сгенерировать HTML-отчёт о покрытии
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
# Открыть отчёт: open coveragereport/index.html
```

##  Мониторинг

- **Health Check**: http://localhost:5001/health
- **Детальный Health Check**: http://localhost:5001/health/detailed
- **Swagger**: http://localhost:5001/swagger
- **Логи**: `docker-compose logs -f`

##  Безопасность и конкурентность

- **Idempotency-Key** — защита от повторных покупок (хранится в БД)
- **RowVersion** — optimistic concurrency для скинов
- **Soft delete** — с глобальным фильтром в EF Core
- **X-User-Id** — мок-авторизация для демо
- **Транзакции** — атомарность операций покупки

##  Особенности реализации

### Внешнее API (CoinGecko)
-  HttpClientFactory с типизированным клиентом
-  Таймаут 5 секунд
-  Обработка ошибок (сетевые, JSON, статусы)
-  Rate limit не превышается благодаря кэшированию

### Кэширование курса BTC
-  IMemoryCache с TTL 60 секунд
-  Fallback до последнего успешного значения (10 минут)
-  При отсутствии свежего fallback → 503 Service Unavailable
-  Endpoint /api/rates/btc-usd с метаданными (source, age)

### EF Core
-  Миграции с автоматическим применением при старте
-  Seeding начальных скинов
-  AsNoTracking для чтения каталогов
-  Глобальный фильтр для soft delete
-  Индексы на часто используемые поля

### Обработка ошибок
-  Централизованное middleware
-  ProblemDetails (RFC 7807)
-  400 — Validation errors
-  404 — Not found
-  409 — Skin unavailable
-  503 — External service unavailable

## Автор

**fairwix**  
GitHub: [@fairwix](https://github.com/fairwix)


# CI/CD test
