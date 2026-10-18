# University Course Management API

REST Web API на **ASP.NET Core 8** для управления студентами, преподавателями, курсами и записями студентов на курсы.
Курс: CSE 5032 «Разработка веб-сервисов», Задание 1 (Модули 1–4).

## Технологии
ASP.NET Core Web API · Entity Framework Core 8 · PostgreSQL · AutoMapper · Swagger (Swashbuckle) · ILogger

## Архитектура

```
Client (Web / Mobile / Desktop / Postman / Swagger)
  ↓ HTTP (JSON)
Middleware (ExceptionHandlingMiddleware)
  ↓
Controller        — только HTTP: маршруты, статус-коды, ReturnResult
  ↓
Service           — бизнес-правила, логирование, DTO <-> Entity (AutoMapper)
  ↓
Repository        — доступ к данным (IStudentRepository и т. д.)
  ↓
Entity Framework Core (ApplicationDbContext, DbSet<T>)
  ↓
Database (PostgreSQL)
```

### Структура проекта
```
UniversityApi
├── Controllers/        Students, Teachers, Courses, Enrollments, Diagnostics
├── Models/             Student, Teacher, Course, Enrollment (Entity)
├── DTO/                Students / Teachers / Courses / Enrollments
├── Data/               ApplicationDbContext, DbSeeder
├── Repositories/       Interfaces + Implementations
├── Services/           Interfaces + Implementations (бизнес-логика)
├── Mapping/            MappingProfile (AutoMapper)
├── Middleware/         ExceptionHandlingMiddleware
├── Validation/         PastDateAttribute
├── Common/             ReturnResult<T>, ErrorCodes
├── database/init.sql   схема + тестовые данные
├── Program.cs
└── appsettings.json
```

## Запуск

1. Установить .NET 8 SDK и Docker (или локальный PostgreSQL).
2. Поднять БД:
   ```bash
   docker compose up -d
   ```
   (строка подключения — `appsettings.json` → `ConnectionStrings:DefaultConnection`)
3. Запустить API:
   ```bash
   dotnet restore
   dotnet run
   ```
   При старте БД и таблицы создаются автоматически (`EnsureCreated`), тестовые данные добавляет `DbSeeder`.
   Вместо этого можно выполнить `database/init.sql` на пустой базе.
4. Открыть Swagger: **http://localhost:5000/swagger**

## Endpoints

| Метод | URL | Описание | Успех | Ошибки |
|---|---|---|---|---|
| GET | /api/students | Все студенты | 200 | |
| GET | /api/students/{id} | Студент по ID | 200 | 404 |
| GET | /api/students/{id}/courses | Студент с курсами и оценками | 200 | 404 |
| POST | /api/students | Создать студента | 201 | 400, 409 (email) |
| PUT | /api/students/{id} | Изменить студента | 200 | 400, 404, 409 |
| DELETE | /api/students/{id} | Удалить студента (+ его записи) | 200 | 404 |
| GET/POST/PUT/DELETE | /api/teachers[/{id}] | CRUD преподавателей | 200/201 | 400, 404, 409 (email; удаление при наличии курсов) |
| GET | /api/courses?name=&teacherId=&minCredits=&maxCredits= | Курсы с фильтрацией | 200 | 400 |
| GET | /api/courses/{id} | Курс по ID | 200 | 404 |
| POST | /api/courses | Создать курс | 201 | 400, 404 (преподаватель) |
| PUT | /api/courses/{id} | Изменить курс | 200 | 400, 404 |
| DELETE | /api/courses/{id} | Удалить курс | 200 | 404, 409 (есть записи) |
| GET | /api/enrollments?studentId=&courseId= | Список записей | 200 | |
| GET | /api/enrollments/{id} | Запись по ID | 200 | 404 |
| POST | /api/enrollments | Записать студента на курс | 201 | 400, 404, 409 (дубль) |
| PUT | /api/enrollments/{id}/grade | Изменить оценку (0–100) | 200 | 400, 404 |
| DELETE | /api/enrollments/{id} | Удалить студента с курса | 200 | 404 |
| GET | /api/diagnostics/error | Демо ошибки 500 (только для защиты) | | 500 |

## Единый формат ответа

```json
{
  "statusCode": 404,
  "isSuccess": false,
  "result": null,
  "errorCode": "STUDENT_NOT_FOUND",
  "errorMessage": "Student not found",
  "traceId": "0HN..."
}
```
При ошибке валидации (400) добавляется поле `errors` со списком проблем по полям.

Коды ошибок: `VALIDATION_ERROR`, `STUDENT_NOT_FOUND`, `TEACHER_NOT_FOUND`, `COURSE_NOT_FOUND`, `ENROLLMENT_NOT_FOUND`,
`STUDENT_EMAIL_EXISTS`, `TEACHER_EMAIL_EXISTS`, `ALREADY_ENROLLED`, `TEACHER_HAS_COURSES`, `COURSE_HAS_ENROLLMENTS`, `INTERNAL_SERVER_ERROR`.

## Бизнес-правила

| № | Правило | Результат |
|---|---|---|
| 1 | Нельзя записать студента на несуществующий курс | 404 `COURSE_NOT_FOUND` |
| 2 | Нельзя записать несуществующего студента | 404 `STUDENT_NOT_FOUND` |
| 3 | Нельзя записать студента на один курс дважды | 409 `ALREADY_ENROLLED` (проверка в сервисе + уникальный индекс в БД) |
| 4 | Курс не может ссылаться на несуществующего преподавателя | 404 `TEACHER_NOT_FOUND` |
| 5 | Несуществующий объект никогда не возвращается как успех | 404 + `isSuccess: false` |

## Конфигурация EF Core (три способа)
- **Conventions** — первичный ключ `Id`, имена таблиц/колонок, обязательность non-nullable свойств.
- **Data Annotations** — `[Required]`, `[MaxLength]` в Models.
- **Fluent API** — связи и FK, `OnDelete`, уникальные индексы (`Email`, `StudentId+CourseId`), `HasPrecision` для оценки.

Поведение удаления: Student → Enrollments = **Cascade**; Teacher → Courses и Course → Enrollments = **Restrict** (сервис возвращает 409 вместо ошибки БД).

## Dependency Injection — lifetime
Все репозитории и сервисы зарегистрированы как **Scoped**, потому что `ApplicationDbContext` (AddDbContext) — Scoped:
один экземпляр на HTTP-запрос. Singleton-сервис, зависящий от Scoped DbContext, привёл бы к captive dependency,
а Transient создавал бы лишние экземпляры и разные DbContext-ы внутри одного запроса не давали бы единой транзакционной единицы.
AutoMapper регистрируется как Singleton (stateless, потокобезопасный) самим `AddAutoMapper`.

## Логирование (ILogger)
- `Information` — создание / изменение / удаление объектов;
- `Warning` — объект не найден, нарушение бизнес-правил, ошибка валидации;
- `Error` — необработанные исключения (полный stack trace — только в лог, клиенту отдаётся безопасный 500).

## Сценарии для защиты
Готовые запросы — в файле `demo-requests.http` (VS / Rider / VS Code REST Client) или повторить через Swagger / Postman:
список студентов → создание → получение по ID → изменение → создание преподавателя → создание курса →
запись на курс → повторная запись (409) → студент с курсами → фильтр курсов → удаление →
несуществующий объект (404) → ошибка валидации (400) → ошибка 500 → просмотр логов в консоли.

## Сдача
```bash
git init
git add .
git commit -m "University Course Management API"
git remote add origin <URL вашего репозитория>
git push -u origin main
```
