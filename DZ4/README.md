# HW4 — Асинхронное межсервисное взаимодействие (Orders + Payments)

Проект реализует асинхронное взаимодействие между микросервисами с гарантией
корректной обработки событий при at-least-once доставке сообщений.

## Архитектура

Содержание репозитория:
- /gateway — nginx-прокси (прокси REST и WebSocket)
- /orders-service — Orders API (создание заказов, outbox, WS push)
- /payments-service — Payments API (accounts, topup, inbox/outbox, ledger)
- /frontend — простая веб-страница для демо (index.html)
- docker-compose.yml — весь стек запускается одной командой


Все сервисы поднимаются одной командой через Docker Compose.
---

Ключевые идеи и достижения (что соответствует требованиям)
- Отдельные микросервисы: Orders и Payments
- Асинхронный обмен через RabbitMQ (at-least-once)
- Transactional Outbox в Orders (с фоновым publisher)
- Transactional Inbox + Outbox в Payments
- Exactly-once (effectively) для списания средств:
    - inbox dedup по EventId
    - уникальность списаний в ledger (уникальный индекс on (order_id, type))
    - атомарные транзакции для ledger + баланс
- WebSocket push уведомления о финальном статусе заказа (/orders/ws)
- Фронтенд в docker-compose и доступ через gateway
- Swagger (в сервисах) + .http/.postman для ручного теста
- Dockerized (Dockerfile + docker-compose)

---

## Технологии

- Backend: C# / .NET 8, ASP.NET Core Web API
- ORM: Entity Framework Core + Npgsql
- Broker: RabbitMQ
- DB: PostgreSQL (orders_db, payments_db)
- Инфраструктура: Docker, Docker Compose, nginx gateway
- Frontend: static HTML + JS (index.html)

---

## Exactly Once Strategy

Брокер сообщений (RabbitMQ) гарантирует доставку **at-least-once**,  
поэтому система обязана быть **идемпотентной**.

Для этого используются следующие паттерны:

### Outbox
- События сохраняются в БД сервиса вместе с бизнес-данными
- Фоновый publisher читает outbox и публикует события в брокер
- Исключает потерю событий между БД и брокером

### Inbox
- Каждый сервис хранит идентификаторы обработанных событий
- Повторно пришедшие события игнорируются
- Исключает повторную обработку

### Idempotent Business Logic
- В Payments Service списание средств защищено уникальными ограничениями
- Деньги не могут быть списаны дважды за один заказ

Эта комбинация обеспечивает **эффективный exactly-once processing на уровне приложения**.

---

## Business Flow: Create Order → Payment

### Основной сценарий

1. Клиент отправляет `POST /orders`
2. Orders Service:
   - создаёт заказ со статусом `PAYMENT_PENDING`
   - сохраняет событие `PaymentRequested` в outbox
3. Outbox publisher:
   - публикует `PaymentRequested` в RabbitMQ
4. Payments Service:
   - принимает событие
   - проверяет inbox (защита от дублей)
   - списывает деньги идемпотентно
   - публикует `PaymentSucceeded` или `PaymentFailed`
5. Orders Service:
   - принимает результат оплаты
   - обновляет статус заказа (`PAID` или `PAYMENT_FAILED`)
6. Клиент получает статус через `GET /orders/{id}`
   - либо через WebSocket (бонус)

---

## Data Model

### Orders Service Database

#### orders
- `order_id` UUID (PK)
- `user_id` TEXT
- `amount` NUMERIC(18,2)
- `status` TEXT  
  (`PAYMENT_PENDING`, `PAID`, `PAYMENT_FAILED`)
- `created_at` TIMESTAMPTZ

#### outbox
- `event_id` UUID (PK)
- `event_type` TEXT
- `payload_json` JSONB
- `created_at` TIMESTAMPTZ
- `published_at` TIMESTAMPTZ NULL
- `publish_attempts` INT

#### inbox
- `event_id` UUID (PK)
- `received_at` TIMESTAMPTZ

---

### Payments Service Database

#### accounts
- `user_id` TEXT (PK)
- `balance` NUMERIC(18,2)
- `version` INT (optimistic locking)

#### inbox
- `event_id` UUID (PK)
- `received_at` TIMESTAMPTZ

#### ledger
- `tx_id` UUID (PK)
- `order_id` UUID
- `user_id` TEXT
- `type` TEXT (`TOPUP`, `DEBIT`)
- `amount` NUMERIC(18,2)
- `status` TEXT (`SUCCESS`, `FAILED`)
- `created_at` TIMESTAMPTZ

**Ограничение идемпотентности:**
- уникальность списания средств по заказу  
  (`UNIQUE(order_id)` или `UNIQUE(order_id, type)` для `type = 'DEBIT'`)

---

## Run

```bash
docker compose up --build
