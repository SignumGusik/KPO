# Антиплагиат

Это учебный проект — минимальная микросервисная система для приёма студентских работ, их хранения и запуска простого антиплагиат‑анализатора с генерацией облака слов. Система состоит из трёх сервисов, всё разворачивается через Docker Compose.

---

## Содержание README
- Архитектура и границы сервисов
- Порты и тома
- Как запустить
- Swagger / Postman / примеры запросов
- Алгоритм обнаружения плагиата (реализация)
- Схемы БД / миграции
- Обработка ошибок и поведение при сбоях
- Ограничения

---

## Архитектура и границы сервисов

Система состоит из трёх микросервисов:

- **File Storing Service** — отвечает за приём и хранение файлов с работами студентов.
- **File Analysis Service** — отвечает за анализ работ и хранение отчётов (антиплагиат, баллы и т.п.).
- **API Gateway** — единая точка входа для клиентов (студент/преподаватель), проксирует запросы к другим сервисам.

### File Storing Service

**Ответственность:**  
Принимает файлы работ от студентов, сохраняет их и ведёт учёт в базе данных.

**Данные и таблицы**

- Таблица `Works`:
    - `id` 
    - `studentId`
    - `assignmentId`
    - `createdAt`
    - `pathToFile`
    - `hash`

**Публичное API сервиса**

- `POST /works` — загрузка новой работы.
    - Вход: файл + метаданные (`studentId`, `assignmentId`, `title`, `timestamp`).
    - Выход: `workId` и основные метаданные.
- `GET /works/{id}` — получить информацию о работе.
    - Выход: JSON с метаданными (без содержимого файла).
- `GET /works/{id}/content` — отдать содержимое файла.
    - Используется другими сервисами (в первую очередь File Analysis Service) для анализа.

**Синхронные взаимодействия**

- Принимает запросы от **API Gateway**.
- Отдаёт файл по `GET /works/{id}/content` для **File Analysis Service`.

### File Analysis Service

**Ответственность:**  
Запускает анализ работы (антиплагиат, подсчёт баллов, построение word cloud), хранит результаты и отдаёт отчёты.

**Данные и таблицы**

- Таблица `Reports`:
    - `id` 
    - `workId` (FK на `Works.id` из File Storing Service)
    - `plagiarismFlag` (bool / enum)
    - `score` (числовой балл)
    - `status` (например: `Pending`, `InProgress`, `Completed`, `Failed`)
    - `createdAt`
    - `wordCloudUrl` (ссылка на картинку с облаком слов)

**Публичное API сервиса**

- `POST /analysis` — запуск анализа работы.
    - Вход: `workId`
    - Внутри:
        1. По `workId` запрашивает контент файла через `GET /works/{id}/content` у File Storing Service.
        2. Выполняет анализ (по описанному в README алгоритму).
        3. Сохраняет результат в таблице `Reports`.
        4. Возвращает информацию о созданном отчёте (или статус запуска).

- `GET /works/{workId}/reports` — получить список отчётов по конкретной работе.
    - Выход: массив отчётов (id, статус, флаг плагиата, score и т.п.).

**Синхронные взаимодействия**

- Принимает запросы от **API Gateway** (`POST /analysis` и `GET /works/{workId}/reports` через прокси).
- Синхронно запрашивает файл у **File Storing Service** по HTTP (`GET /works/{id}/content`).

### API Gateway

**Ответственность:**  
Единая точка входа для клиентов (студент / преподаватель). Не хранит бизнес-данные, только маршрутизирует запросы и оркестрирует вызовы File Storing и File Analysis.

**Публичное API для клиентов**

- `POST /gateway/works` — студент загружает работу.
    - Вход: файл + метаданные (`studentId`, `assignmentId`, `title`).
    - Внутренний сценарий:
        1. Вызывает `POST /works` в **File Storing Service**.
        2. Получает `workId`.
        3. Вызывает `POST /analysis` в **File Analysis Service**, передаёт `workId`.
        4. Возвращает клиенту `workId` и, например, статус анализа (`analysisStarted: true`).

- `GET /gateway/works/{workId}/reports` — преподаватель получает отчёты по работе.
    - Внутренний сценарий:
        1. Делает запрос `GET /works/{workId}/reports` в **File Analysis Service**.
        2. Возвращает клиенту список отчётов.

**Синхронные взаимодействия**

- Принимает все внешние HTTP-запросы (от клиента).
- Вызывает:
    - **File Storing Service** по HTTP (`POST /works`).
    - **File Analysis Service** по HTTP (`POST /analysis`, `GET /works/{workId}/reports`).

### Сценарий 1: студент отправляет работу на проверку

1. Клиент (студент) отправляет запрос `POST /gateway/works` с файлом и метаданными.
2. API Gateway вызывает `POST /works` в **File Storing Service**:
    - сервис сохраняет файл,
    - создаёт запись в таблице `Works`,
    - возвращает `workId`.
3. API Gateway вызывает `POST /analysis` в **File Analysis Service**, передаёт `workId`.
4. File Analysis Service:
    - запрашивает содержимое файла через `GET /works/{workId}/content` у File Storing Service,
    - выполняет анализ (антиплагиат, score, word cloud),
    - создаёт запись в таблице `Reports`.
5. API Gateway возвращает клиенту `workId` и информацию о запуске анализа.

### Сценарий 2: преподаватель смотрит отчёты по работе

1. Клиент (преподаватель) отправляет запрос `GET /gateway/works/{workId}/reports`.
2. API Gateway делает запрос `GET /works/{workId}/reports` в **File Analysis Service**.
3. File Analysis Service выбирает все отчёты из таблицы `Reports` по `workId` и отдаёт их Gateway.
4. API Gateway возвращает клиенту список отчётов в виде JSON.

### Сводная таблица по сервисам

| Сервис              | Ответственность                         | Таблицы          | Публичные эндпоинты                      | Внутренние вызовы                           |
|---------------------|-----------------------------------------|------------------|------------------------------------------|---------------------------------------------|
| File Storing        | Хранение файлов и метаданных работ     | `Works`          | `POST /works`, `GET /works/{id}`, `GET /works/{id}/content` | — (только отвечает на запросы)             |
| File Analysis       | Анализ работ и хранение отчётов        | `Reports`        | `POST /analysis`, `GET /works/{workId}/reports` | `GET /works/{id}/content` → File Storing   |
| API Gateway         | Единая точка входа для клиентов        | —                | `POST /gateway/works`, `GET /gateway/works/{workId}/reports` | `POST /works` → File Storing; `POST /analysis`, `GET /works/{workId}/reports` → File Analysis |

## Порты (локально через Docker)

- ApiGateway: http://localhost:5090
- FileStorageService: http://localhost:5020
- FileAnalysisService: http://localhost:5145
- PostgreSQL для FileStorage: контейнер `filestorage-db` (5432 внутри сети)
- PostgreSQL для FileAnalysis: контейнер `fileanalysis-db` (5432 внутри сети)

Файлы работ хранятся в Docker volume `filestorage_files` смонтированном в контейнере FileStorageService по пути `/app/files`.

---

## Как запустить (быстро)

Требования:
- Docker Engine (версия, поддерживающая docker compose v2)
- docker compose CLI (или Docker Desktop)

Запуск:
1. В корне репозитория:
   ```bash
   docker compose up --build
   ```
2. Подождите, пока контейнеры инициализируются. Миграции применяются автоматически (см. раздел миграций).  
3. Swagger UI:
   - ApiGateway: http://localhost:5090/swagger
   - FileStorageService: http://localhost:5020/swagger
   - FileAnalysisService: http://localhost:5145/swagger

Если нужно запустить в фоне:
```bash
docker compose up -d --build
```

Остановить и удалить контейнеры:
```bash
docker compose down
```

---

## Volumes и сохранность файлов

В docker‑compose используется именованный volume:
- `filestorage_files` → монтируется в контейнер FileStorageService в `/app/files`.

Чтобы увидеть содержимое volume (на Linux/Mac), можно временно запустить контейнер и посмотреть:
```bash
docker run --rm -v <project>_filestorage_files:/data alpine ls -la /data
```
(замените `<project>` на префикс вашего docker compose проекта, по умолчанию имя папки).

---

## Swagger / Postman / Примеры запросов

Рекомендуется использовать Swagger UI для ручной проверки. Ниже — curl‑примеры, покрывающие основной user‑flow.

1) Загрузка работы (через ApiGateway — он проксирует в FileStorage):
```bash
curl -v \
  -F "file=@/path/to/your/file.txt" \
  -F "studentId=11111111-1111-1111-1111-111111111111" \
  -F "assignmentId=22222222-2222-2222-2222-222222222222" \
  http://localhost:5090/works
```
Успешный ответ (пример):
```json
{
  "workId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "studentId": "11111111-1111-1111-1111-111111111111",
  "assignmentId": "22222222-2222-2222-2222-222222222222",
  "createdAt": "2025-12-13T12:34:56.789Z"
}
```

2) Запрос отчётов по работе (через ApiGateway):
```bash
curl http://localhost:5090/works/{workId}/reports
```

3) Запуск анализа напрямую (для отладки) на FileAnalysisService:
```bash
curl -X POST -H "Content-Type: application/json" \
  -d '{"workId":"aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee"}' \
  http://localhost:5145/analysis
```

---

## Алгоритм обнаружения плагиата (реализация)

Реализован в `FileAnalysisService.UseCases.RunAnalysis.RunAnalysisRequestHandler`.

Кратко:
1. Получаем метаданные работы у FileStorageService (`GET /works/{id}`) — нужны StudentId и AssignmentId.
2. Загружаем содержимое файла (`GET /works/{id}/content`) как byte[].
3. Считаем SHA‑256 по байтам файла — это `DocumentHash`.
   Пример C#:
   ```csharp
   using var sha = SHA256.Create();
   var hashBytes = sha.ComputeHash(fileBytes);
   var documentHash = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
   ```
4. Берём предыдущие отчёты для того же `AssignmentId`, исключая текущего студента.
5. Сравниваем `DocumentHash` с `DocumentHash` в предыдущих отчётах:
   - Если есть совпадение (равные хэши) — считаем плагиатом:
     - `PlagiarismFlag = true`
     - `SimilarWorkId` = id найденной предыдущей работы
     - `Score = 0.0`
     - `Status = "PlagiarismDetected"`
   - Иначе — `Status = "Completed"`, `Score = 100.0`, `PlagiarismFlag = false`.
6. Формируем безопасный URL для word cloud (quickchart) на основе очищенного текста (sanitize), при этом ограничиваем длину входного текста, чтобы итоговый URL не превышал 1024 символа.
7. Сохраняем отчёт в таблицу `Reports` с полями: DocumentHash, WordCloudUrl и т.д.

---

## Схемы БД и миграции

FileStorageService:
- Таблица `Works`:
  - Id uuid PK
  - StudentId uuid
  - AssignmentId uuid
  - CreatedAt timestamp with time zone
  - FilePath text
  - Hash varchar(256) nullable

FileAnalysisService:
- Таблица `Reports`:
  - Id uuid PK
  - WorkId uuid
  - StudentId uuid
  - AssignmentId uuid
  - PlagiarismFlag boolean
  - SimilarWorkId uuid nullable
  - Score double
  - Status text
  - WordCloudUrl varchar(1024) nullable
  - CreatedAt timestamp with time zone
  - DocumentHash varchar(64) nullable

Миграции:
- В проекте есть миграции EF Core (папки `Infrastructure/Migrations`).
- Миграции применяются автоматически при старте через:
  - либо `MigrationRunner` (IHostedService) — для FileAnalysisService,
  - либо `db.Database.Migrate()` в Program.cs — для FileStorageService.
- Если автоматическое применение выключено, выполнить вручную:
  ```bash
  dotnet ef database update --project FileAnalysisService.Infrastructure --startup-project FileAnalysisService.Host
  ```

---

## Обработка ошибок и поведение при сбоях

- ApiGateway:
  - Если FileStorageService недоступен при попытке загрузки — возвращает 503 с сообщением `"FileStorageService unavailable"`.
  - Если FileAnalysisService недоступен — загрузка файла НЕ откатывается; ApiGateway вернёт, что работа загружена, но `analysisStarted=false`. Это поведение специально: сохранение данных не блокируется падением аналитического сервиса (MVP).
- FileAnalysisService:
  - При отсутствии метаданных или содержимого создаёт `Report` со статусом `Failed:Meta` или `Failed:Content`.
- БД:
  - Healthchecks есть для PostgreSQL контейнеров (pg_isready).

---

## Ограничения

Ограничения текущей реализации:
- Алгоритм плагиата на основе SHA‑256 обнаруживает только точные совпадения содержимого.
- Чувствителен к кодировке/форматированию/метаданным. Малейшее изменение в тексте приведёт к отличающемуся хэшу.
- Нет проверки типов файлов / антивирусной проверки.


---

## Примеры ответов API (успех / ошибка)

- Успех загрузки:
```json
{
  "workId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "analysisStarted": true,
  "analysisStatus": "Completed",
  "message": "Work uploaded and analysis started"
}
```

- Анализ не запущен (FileAnalysis недоступен):
```json
{
  "workId": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "analysisStarted": false,
  "analysisStatus": null,
  "message": "Work uploaded, but analysis is not started yet. Try again later."
}
```

- Ошибка сервиса (пример 503):
```json
{
  "error": "FileStorageService unavailable",
  "details": "Reason text..."
}
```

---

