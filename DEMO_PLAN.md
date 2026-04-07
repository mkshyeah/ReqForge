# ReqForge — План демонстрации курсовой

## Подготовка перед показом

1. **Запусти приложение** через Rider или `dotnet run`
2. **Groq API key** — вставь заранее на вкладке AI (ключ `gsk_...` с console.groq.com)
3. **Убедись**, что есть интернет (нужен для запросов и AI)
4. **Переключи тему** на тёмную — выглядит эффектнее на проекторе

---

## Часть 1: Регистрация и вход (1 мин)

**Что показываем**: система авторизации, хэширование паролей, пользовательские данные

1. В верхней панели введи:
   - **Username**: `demo`
   - **Password**: `demo123`
2. Нажми **Register** → появится "Registration successful"
3. Нажми **Login** → имя пользователя отобразится, кнопки коллекций разблокируются
4. **Что сказать**: "У каждого пользователя свои коллекции, история и среды. Пароли хранятся в виде SHA-256 хэша с солью"

---

## Часть 2: Базовый GET-запрос (2 мин)

**Что показываем**: отправка запроса, статус, время, форматирование JSON

1. **Метод**: `GET`
2. **URL**: `https://jsonplaceholder.typicode.com/posts/1`
3. Нажми **Send**
4. **Ожидаемый ответ** (200 OK, ~50ms):
```json
{
  "userId": 1,
  "id": 1,
  "title": "sunt aut facere repellat provident occaecati excepturi optio reprehenderit",
  "body": "quia et suscipit\nsuscipit..."
}
```
5. Обрати внимание на:
   - **Статус-бейдж**: `200 OK · XXms · XX bytes`
   - **Вкладка Body** — JSON отформатирован
   - **Вкладка Headers** — заголовки ответа
   - **History** — запрос записался в историю

---

## Часть 3: POST-запрос с JSON body (2 мин)

**Что показываем**: разные HTTP-методы, типы тела, заголовки

1. **Метод**: `POST`
2. **URL**: `https://jsonplaceholder.typicode.com/posts`
3. **Вкладка Body** → выбери тип: `json`
4. **Вставь тело**:
```json
{
  "title": "ReqForge Demo",
  "body": "Testing POST request from our app",
  "userId": 1
}
```
5. Нажми **Send**
6. **Ожидаемый ответ** (201 Created):
```json
{
  "title": "ReqForge Demo",
  "body": "Testing POST request from our app",
  "userId": 1,
  "id": 101
}
```
7. **Что сказать**: "Приложение автоматически добавляет Content-Type: application/json при выборе json"

---

## Часть 4: Заголовки и Query Params (1 мин)

**Что показываем**: кастомные заголовки, параметры запроса

1. **Метод**: `GET`
2. **URL**: `https://jsonplaceholder.typicode.com/posts`
3. **Вкладка Params** → нажми **Add Param**:
   - **Key**: `userId` / **Value**: `1`
4. Нажми **Send** → получишь 10 постов пользователя 1
5. **Вкладка Headers** → добавь:
   - **Key**: `X-Custom-Header` / **Value**: `ReqForge-Demo`
6. **Что сказать**: "Параметры автоматически добавляются к URL, заголовки отправляются с каждым запросом"

---

## Часть 5: Авторизация запросов (2 мин)

**Что показываем**: 4 типа авторизации

1. Перейди на **вкладку Auth**
2. **Bearer Token**:
   - Выбери тип: `Bearer Token`
   - Введи: `my-secret-token-123`
   - **Метод**: `GET`, **URL**: `https://httpbin.org/bearer`
   - **Send** → ответ `200 OK` с `"authenticated": true`
3. **Basic Auth**:
   - Выбери тип: `Basic Auth`
   - **Username**: `user` / **Password**: `passwd`
   - **URL**: `https://httpbin.org/basic-auth/user/passwd`
   - **Send** → `200 OK`, `"authenticated": true`
4. **API Key**:
   - Выбери тип: `API Key`
   - **Header name**: `X-API-Key` / **Value**: `demo-key-456`
   - Покажи что заголовок добавляется автоматически
5. Верни тип на `None`
6. **Что сказать**: "Авторизация сохраняется вместе с запросом в коллекции"

---

## Часть 6: Коллекции (2 мин)

**Что показываем**: создание, сохранение, именование, удаление

1. Нажми **New Collection** в сайдбаре → введи имя: `Demo API`
2. Сделай GET на `https://jsonplaceholder.typicode.com/users/1`
3. Нажми **Save Current** → запрос сохранится в коллекцию `Demo API`
4. Сделай ещё запрос: POST на `https://jsonplaceholder.typicode.com/posts` (с body из Части 3)
5. **Save Current** → теперь 2 запроса в коллекции
6. В сайдбаре раскрой коллекцию → кликни на сохранённый GET → данные загрузятся
7. **ПКМ на коллекции** → "Rename Collection" → переименуй в `JSONPlaceholder`
8. Создай ещё одну коллекцию `Temp`, затем **ПКМ → Delete Collection**
9. **Что сказать**: "Коллекции хранятся в SQLite базе, привязаны к пользователю"

---

## Часть 7: Переменные среды (2 мин)

**Что показываем**: среды, переменные, подстановка {{variable}}

1. Перейди на **вкладку Environments**
2. Нажми **New Environment** → имя: `Production`
3. Добавь переменные:
   - `base_url` = `https://jsonplaceholder.typicode.com`
   - `user_id` = `1`
4. В верхней панели выбери среду `Production`
5. В URL введи: `{{base_url}}/users/{{user_id}}`
6. Нажми **Send** → придёт пользователь с id=1
7. Поменяй `user_id` на `5` → Send → другой пользователь
8. **Что сказать**: "Как в Postman — переменные позволяют быстро переключаться между серверами: dev, staging, production"

---

## Часть 8: Поиск по коллекциям (30 сек)

**Что показываем**: фильтрация в реальном времени

1. В сайдбаре в поле **Search** введи `post`
2. Отфильтруются только запросы с "post" в имени/URL
3. Очисти поиск → всё вернётся
4. **Что сказать**: "Поиск по имени коллекции и URL запросов"

---

## Часть 9: Тесты ответа (2 мин)

**Что показываем**: автоматическая проверка ответов

1. **URL**: `https://jsonplaceholder.typicode.com/posts/1`, метод `GET`
2. Перейди на **вкладку Tests** → нажми **Add Test** 3 раза:
   - **Type**: `StatusEquals` / **Expected Value**: `200`
   - **Type**: `BodyContains` / **Expected Value**: `sunt aut facere`
   - **Type**: `TimeLessThanMs` / **Expected Value**: `2000`
3. Нажми **Send**
4. Перейди в **Response → Test Results** → все 3 теста зелёные ✓
5. Поменяй ожидаемый статус на `404` → **Send** → первый тест красный ✗
6. **Что сказать**: "После каждого запроса тесты запускаются автоматически. Можно проверять статус, содержимое body и время ответа"

---

## Часть 10: Генерация кода (2 мин)

**Что показываем**: 4 формата генерации кода из текущего запроса

1. Убедись что есть запрос: POST на `https://jsonplaceholder.typicode.com/posts` с JSON body
2. Перейди в **Response → вкладка Code**
3. Выбери `cURL (bash)` → нажми **Generate** → покажи код
4. Переключи на `cURL (Windows)` → покажи отличия (двойные кавычки, `^`)
5. Переключи на `PowerShell` → `Invoke-RestMethod` с `$headers` hashtable
6. Переключи на `C# HttpClient` → полноценный C# код с `PostAsync`
7. Нажми **Copy** → вставь в блокнот чтоб показать что скопировалось
8. **Что сказать**: "Как в Postman — можно сгенерировать код для любой платформы. Bash для Linux, CMD для Windows, PowerShell и C# для .NET разработчиков"

---

## Часть 11: WebSocket клиент (2 мин)

**Что показываем**: real-time двусторонняя связь

1. Перейди на **вкладку WebSocket**
2. URL уже `wss://echo.websocket.org`
3. Нажми **Connect** → статус: `Connected to echo.websocket.org`
4. В поле сообщения введи: `Hello from ReqForge!`
5. Нажми **Send** → появятся:
   - `[OUT] HH:mm:ss  Hello from ReqForge!`
   - `[IN]  HH:mm:ss  Hello from ReqForge!` (эхо)
6. Отправь JSON:
```
{"event": "test", "data": "курсовая работа"}
```
7. Нажми **Disconnect** → статус: `Disconnected`
8. **Что сказать**: "WebSocket — протокол для real-time коммуникации. Echo-сервер возвращает всё что мы отправляем. Полезно для тестирования чат-приложений, игр, бирж"

---

## Часть 12: AI-помощник (3 мин) ⭐

**Что показываем**: интеграция с нейросетью (Groq/Llama)

### 12.1 Explain Response
1. Сначала сделай GET на `https://jsonplaceholder.typicode.com/posts/1` → получи ответ
2. Перейди на **вкладку AI**
3. Убедись что выбрана модель **Llama 3.3 70B** и вставлен API key
4. Нажми **Explain Response**
5. AI выдаст объяснение на русском: что за статус-код, что содержит тело ответа
6. **Что сказать**: "AI анализирует ответ сервера и объясняет его — полезно для новичков и для дебага сложных API"

### 12.2 Generate JSON Body
1. В поле рядом с кнопкой введи: `user with name, email, age 25 and list of 3 hobbies`
2. Нажми **Generate JSON** → AI сгенерирует JSON и вставит его в Body
3. Перейди на **вкладку Body** → JSON уже там, тип выставлен на `json`
4. **Что сказать**: "Описываешь на человеческом языке какой JSON нужен — AI генерирует структуру"

### 12.3 Import cURL
1. В поле cURL вставь:
```
curl -X POST https://api.example.com/users -H "Authorization: Bearer token123" -H "Content-Type: application/json" -d '{"name": "John", "age": 30}'
```
2. Нажми **Import cURL** → AI распарсит и заполнит:
   - Метод: POST
   - URL: https://api.example.com/users
   - Headers: Authorization, Content-Type
   - Body: JSON
3. **Что сказать**: "Скопировал cURL из документации или DevTools браузера — AI автоматически разложил по полям"

---

## Часть 13: Import/Export (1 мин)

**Что показываем**: обмен коллекциями

1. Нажми **Export** в сайдбаре → сохрани как `demo_export.json`
2. Открой файл в блокноте — покажи структуру JSON
3. Нажми **Import** → выбери тот же файл → коллекции импортируются
4. **Что сказать**: "Можно делиться коллекциями с командой. Формат — JSON, аналогично Postman"

---

## Часть 14: Смена темы (15 сек)

1. Нажми иконку **луны/солнца** в правом верхнем углу
2. Тема переключится между тёмной и светлой
3. **Что сказать**: "Тёмная и светлая тема через MaterialDesign"

---

## Часть 15: Юнит-тесты (1 мин)

**Что показываем**: покрытие тестами

1. Если есть терминал/Rider, запусти:
```
dotnet test --verbosity normal
```
2. **Ожидаемый результат**: все тесты проходят (36+ тестов)
3. Покажи категории:
   - `CodeGeneratorServiceTests` — 36 тестов генерации кода (все 4 формата)
   - `AuthServiceTests` — регистрация, логин, хэширование
   - `ResponseTestLogicTests` — логика проверки ответов
   - `SerializationTests` — сериализация коллекций
   - `HttpResponseResultTests` — модель ответа
   - `ConverterTests` — WPF конвертеры
4. **Что сказать**: "Бизнес-логика покрыта юнит-тестами на xUnit. Тестируется генерация кода, авторизация, сериализация, конвертеры"

---

## Шпаргалка: все URL для демо

| # | Метод | URL | Для чего |
|---|-------|-----|----------|
| 1 | GET | `https://jsonplaceholder.typicode.com/posts/1` | Базовый GET |
| 2 | GET | `https://jsonplaceholder.typicode.com/posts?userId=1` | Query params |
| 3 | POST | `https://jsonplaceholder.typicode.com/posts` | POST с JSON |
| 4 | PUT | `https://jsonplaceholder.typicode.com/posts/1` | Обновление |
| 5 | DELETE | `https://jsonplaceholder.typicode.com/posts/1` | Удаление |
| 6 | GET | `https://jsonplaceholder.typicode.com/users/1` | Данные user |
| 7 | GET | `https://httpbin.org/bearer` | Bearer auth |
| 8 | GET | `https://httpbin.org/basic-auth/user/passwd` | Basic auth |
| 9 | GET | `https://httpbin.org/status/404` | Тест ошибки |
| 10 | WSS | `wss://echo.websocket.org` | WebSocket |

---

## Шпаргалка: тело для POST/PUT

```json
{
  "title": "ReqForge Demo",
  "body": "Testing POST request from our app",
  "userId": 1
}
```

---

## Шпаргалка: cURL для AI Import

```
curl -X POST https://api.example.com/users -H "Authorization: Bearer token123" -H "Content-Type: application/json" -d '{"name": "John", "age": 30}'
```

---

## Шпаргалка: переменные среды

| Variable | Value |
|----------|-------|
| `base_url` | `https://jsonplaceholder.typicode.com` |
| `user_id` | `1` |

URL с подстановкой: `{{base_url}}/users/{{user_id}}`

---

## Порядок показа (оптимальный, ~20 мин)

1. 🔐 Регистрация/вход (1 мин)
2. 📡 GET запрос (2 мин)
3. 📤 POST запрос с JSON (2 мин)
4. 📋 Headers + Query Params (1 мин)
5. 🔑 Авторизация (2 мин)
6. 📁 Коллекции (2 мин)
7. 🌍 Переменные среды (2 мин)
8. 🔍 Поиск (30 сек)
9. ✅ Тесты ответа (2 мин)
10. 💻 Генерация кода (2 мин)
11. 🔌 WebSocket (2 мин)
12. 🤖 AI помощник (3 мин)
13. 📦 Import/Export (1 мин)
14. 🌙 Тема (15 сек)
15. 🧪 Юнит-тесты (1 мин)

**Итого: ~21 минута**

---

## Технологии (для слайда/ответа на вопросы)

- **.NET 8.0** + **WPF** (Windows Presentation Foundation)
- **MVVM** — CommunityToolkit.Mvvm (ObservableProperty, RelayCommand)
- **MaterialDesignThemes** — UI компоненты
- **Entity Framework Core** + **SQLite** — локальная БД
- **System.Net.Http.HttpClient** — HTTP запросы
- **System.Net.WebSockets** — WebSocket клиент
- **Groq API** (Llama 3.3 70B) — AI-интеграция
- **xUnit** — юнит-тесты
- **System.Text.Json** — сериализация

---

## Возможные вопросы и ответы

**Q: Чем отличается от Postman?**
A: ReqForge — десктопное WPF приложение с локальной БД, AI-помощником на Llama, генерацией кода в 4 форматах и WebSocket клиентом. Написано с нуля на C# / .NET 8.

**Q: Как хранятся пароли?**
A: SHA-256 хэш с уникальной солью (RandomNumberGenerator). Пароль в открытом виде нигде не хранится.

**Q: Зачем переменные среды?**
A: Чтобы быстро переключаться между dev/staging/production без изменения каждого запроса.

**Q: Как работает AI?**
A: Отправляем промпт на Groq API (бесплатный, использует модель Llama 3.3 70B). API OpenAI-совместимый. Три функции: объяснение ответа, генерация JSON, парсинг cURL.

**Q: Какие паттерны используются?**
A: MVVM (Model-View-ViewModel), Dependency Injection (через конструктор), Repository паттерн (сервисы хранения), Strategy (разные типы авторизации).
