# Variables

| Key | Value |
|-----|-------|
| `base_url` | `https://jsonplaceholder.typicode.com` |
| `post_id` | `1` |
| `user_id` | `1` |
| `bearer_token` | `my-secret-token-123` |
| `basic_user` | `user` |
| `basic_password` | `passwd` |
| `kmg_base` | `http://localhost:5062` |
| `field_id` | `1` |

---

# JSONPlaceholder

`GET` `{{base_url}}/posts/{{post_id}}`

`GET` `{{base_url}}/posts?userId={{user_id}}`

`POST` `{{base_url}}/posts`

```json
{
  "title": "ReqForge Demo",
  "body": "Testing POST request from our app",
  "userId": 1
}
```

`PUT` `{{base_url}}/posts/{{post_id}}`

```json
{
  "id": 1,
  "title": "ReqForge Demo (updated)",
  "body": "Updated body",
  "userId": 1
}
```

`PATCH` `{{base_url}}/posts/{{post_id}}`

```json
{
  "title": "ReqForge Demo (patched)"
}
```

`DELETE` `{{base_url}}/posts/{{post_id}}`

`GET` `{{base_url}}/users/{{user_id}}`

---

# httpbin

`GET` `https://httpbin.org/bearer`  
Auth Bearer: `{{bearer_token}}`

`GET` `https://httpbin.org/bearer`  
Auth: none

`GET` `https://httpbin.org/basic-auth/user/passwd`  
Auth Basic: `{{basic_user}}` / `{{basic_password}}`

`GET` `https://httpbin.org/headers`

| Header | Value |
|--------|-------|
| `X-API-Key` | `demo-key-456` |

---

# KMG local

`GET` `{{kmg_base}}/api/Fields`

`GET` `{{kmg_base}}/api/Fields/{{field_id}}`

`POST` `{{kmg_base}}/api/Fields`

```json
{
  "name": "Узен",
  "region": "Мангистауская область",
  "isActive": true
}
```

`PUT` `{{kmg_base}}/api/Fields/{{field_id}}`

```json
{
  "name": "Тенгиз",
  "region": "Атырауская область",
  "isActive": true
}
```

`DELETE` `{{kmg_base}}/api/Fields/{{field_id}}`

---

# WebSocket

`wss://echo.websocket.org`

---

# Tests (example)

| Type | Expected |
|------|----------|
| StatusEquals | `200` |
| BodyContains | `sunt aut facere` |
| TimeLessThanMs | `3000` |
