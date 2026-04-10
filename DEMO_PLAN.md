# ReqForge — сценарий защиты и шпаргалка

Открой в Rider: двойной клик по `DEMO_PLAN.md`.

---

## Часть A — основной показ (~7 минут): ReqForge + публичные API

**Фокус:** клиент ReqForge (запросы, коллекции, среды, тесты, история, код).  
**Авторизацию** показывай на **httpbin.org** (как раньше): без токена не проходит, с токеном — проходит.  
**CRUD и JSON** — на **jsonplaceholder.typicode.com** (знакомый всем сценарий, не нужен свой backend).

**Нужен интернет** (и отдельно ключ Groq, если показываешь AI из части C).

### A0. Чеклист перед входом

| Проверка | Действие |
|----------|----------|
| Интернет | Есть |
| ReqForge | Запускается, залогинен тестовый пользователь (`demo` / `demo123` или свой) |
| Коллекция | Есть коллекция с запросами из таблицы ниже |
| Среда | `base_url` = `https://jsonplaceholder.typicode.com` |
| (Опционально) Локальный KMG API | Только если хочешь 1 минуту «свой CRUD» — см. часть B |

### A1. Переменные среды (пример)

| Переменная | Значение |
|------------|----------|
| `base_url` | `https://jsonplaceholder.typicode.com` |
| `post_id` | `1` |

URL: `{{base_url}}/posts/{{post_id}}`

### A2. Порядок в ReqForge (рекомендуемый)

1. **GET** `{{base_url}}/posts/1` — базовый запрос, JSON, время, **History**.
2. **POST** `{{base_url}}/posts` — тело JSON (как в старом плане), **201**.
3. **Params:** GET `{{base_url}}/posts` + `userId` = `1`.
4. **Auth (важно для простоты):**
   - **Bearer:** GET `https://httpbin.org/bearer`, в ReqForge вкладка **Auth** → Bearer → `my-secret-token-123` → **Send** → `200`.
   - Убери токен / поставь неверный → покажи, что **не проходит** (ожидаемая ошибка).
   - По желанию: **Basic** `https://httpbin.org/basic-auth/user/passwd`, user / passwd.
5. **Tests:** у GET `posts/1` — `StatusEquals` `200`, `BodyContains` фрагмент из ответа, `TimeLessThanMs` `3000` → **Test Results**.
6. **Code:** сгенерировать cURL или C# для того же запроса.
7. Одна фраза: «Тот же клиент работает с любым REST API; сегодня публичные учебные сервисы, плюс у меня в репозитории есть простой локальный CRUD для примера бэкенда».

### A3. Что говорить (коротко)

| Момент | Фраза |
|--------|--------|
| Старт | «ReqForge — десктопный REST-клиент: коллекции, среды, история, тесты ответа, генерация кода». |
| Про auth | «Здесь не наш токен, а стандартный демо-сервис: так наглядно видно, что без заголовка авторизации запрос отклоняется». |
| Про JSONPlaceholder | «Типичный CRUD-контракт: GET/POST, тело JSON — как в реальных интеграциях». |
| Финал | «Локально у меня развёрнут минимальный API месторождений только чтобы показать свой backend без JWT; на защите могу не открывать, если не хватает времени». |

---

## Часть B — опционально (~1 мин): локальный API «КазМунайГаз» только CRUD

**Когда:** если спросят «а свой сервер есть?» или хочешь визуально показать изменение данных.

**Что это:** `ReqForge.KmgDemo.API` — **без авторизации**, только **CRUD** по сущности «месторождение», данные в памяти.

### Запуск

Rider → проект `ReqForge.KmgDemo.API` → **Run** → порт из лога (часто `http://localhost:5062`).

### Наглядный «мини-фронт» (без React и без npm)

В браузере открой **`http://localhost:5062/`** — одна статическая страница: таблица + форма.  
Там же видно **добавление / правка / удаление**; под капотом вызовы `GET/POST/PUT/DELETE /api/Fields`.

### Swagger

`http://localhost:5062/swagger` — контракт API.

### ReqForge к этому API (без токена)

| Действие | Запрос |
|----------|--------|
| Список | `GET http://localhost:5062/api/Fields` |
| Одна запись | `GET http://localhost:5062/api/Fields/1` |
| Создать | `POST .../api/Fields` + JSON `name`, `region`, `isActive` |
| Обновить | `PUT .../api/Fields/{id}` |
| Удалить | `DELETE .../api/Fields/{id}` |
| 404 | `GET .../api/Fields/999` |

Файл `ReqForge.KmgDemo.API/ReqForge.KmgDemo.http` в Rider — готовые шаблоны запросов.

---

## Часть C — развёрнутый запасной сценарий (интернет)

Если попросят показать больше: WebSocket, AI, Import/Export, `dotnet test` — см. свёрнутый блок ниже.

<details>
<summary>Развернуть: WebSocket, Groq AI, Import/Export, юнит-тесты…</summary>

### Подготовка

1. ReqForge запущен.
2. **Groq API key** на вкладке AI (`gsk_...`), если показываешь AI.
3. Тема: тёмная — удобнее на проекторе.

### Регистрация / вход в ReqForge

- `demo` / `demo123` → Register → Login.

### JSONPlaceholder (детально)

- GET `https://jsonplaceholder.typicode.com/posts/1`
- POST `https://jsonplaceholder.typicode.com/posts` с телом:

```json
{
  "title": "ReqForge Demo",
  "body": "Testing POST request from our app",
  "userId": 1
}
```

### httpbin — авторизация

- Bearer + `https://httpbin.org/bearer`
- Basic + `https://httpbin.org/basic-auth/user/passwd`

### WebSocket

- `wss://echo.websocket.org` — Connect, сообщение, Disconnect.

### AI (Groq)

- Explain Response, Generate JSON, Import cURL.

### Юнит-тесты

```bash
dotnet test --verbosity normal
```

### Шпаргалка URL

| Метод | URL |
|--------|-----|
| GET | `https://jsonplaceholder.typicode.com/posts/1` |
| POST | `https://jsonplaceholder.typicode.com/posts` |
| GET | `https://httpbin.org/bearer` |
| GET | `https://httpbin.org/basic-auth/user/passwd` |
| WSS | `wss://echo.websocket.org` |

</details>

---

## Технологии (для ответа комиссии)

- **ReqForge:** .NET 8, WPF, MVVM, SQLite, HttpClient, WebSocket, опционально Groq.
- **Демо API (опционально):** ASP.NET Core 8, in-memory CRUD, Swagger, статическая страница в `wwwroot`.

---

## Возможные вопросы

**Почему у локального API нет логина?**  
Чтобы на защите не тратить время на JWT: CRUD и так понятен; авторизацию показываю на httpbin в ReqForge.

**Зачем тогда страница в браузере?**  
Чтобы нетехническому жюри было видно «данные реально меняются», без разбора JSON.

**Чем не Postman?**  
Кратко: десктоп, своя модель коллекций/БД, генерация кода, WebSocket, по проекту — свои фичи (AI и т.д.).
