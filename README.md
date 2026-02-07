# CulinaryRecipes.API

## Local setup (including Mailjet SMTP)

1. Start MongoDB locally (default connection used by this app):
   - `mongodb://localhost:27017`
2. Create a Mailjet account and prepare:
   - `API Key` and `Secret Key`
   - one verified sender email/domain in Mailjet
3. Set required secrets (recommended over committing credentials to `appsettings.json`):

```bash
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:SmtpServer" "in-v3.mailjet.com"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:SmtpPort" "587"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:SmtpUser" "your-mailjet-api-key"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:SmtpPassword" "your-mailjet-secret-key"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:FromEmail" "verified-sender@yourdomain.com"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "SMTP:FromName" "Culinary Recipes"
dotnet user-secrets --project CulinaryRecipes.API/CulinaryRecipes.API.csproj set "Jwt:Key" "YourLongRandomJwtSigningKeyAtLeast32Chars"
```

4. Run the API:

```bash
dotnet run --project CulinaryRecipes.API/CulinaryRecipes.API.csproj
```

5. Test email flow:
   - Register account: `POST /api/Account/register` (confirmation email is sent)
   - Forgot password: `POST /api/Account/forgotPassword` (reset email is sent)

## Messaging OpenAPI + SignalR integration guide

### Where to find API docs

- Swagger/OpenAPI UI: `https://<api-host>/swagger`
- Messaging REST endpoints are under:
  - `api/Messaging`
  - `api/Notifications`
- Authentication: `Authorization: Bearer <jwt-token>`

> SignalR hub methods are not part of OpenAPI schema. They are documented below for frontend integration.

---

## REST payload contracts

### 1) List conversations

- **GET** `/api/Messaging/conversations`
- **Response** `200 OK`

```json
[
  {
    "id": "67ce0ab53ca0f69f633899a2",
    "participantUserIds": ["user-1", "user-2"],
    "createdAt": "2026-02-07T18:24:38.000Z",
    "updatedAt": "2026-02-07T18:25:04.000Z",
    "lastMessagePreview": "Hey, check this video",
    "lastMessageAt": "2026-02-07T18:25:04.000Z"
  }
]
```

### 2) List messages for conversation

- **GET** `/api/Messaging/conversations/{conversationId}/messages?skip=0&take=50`
- **Response** `200 OK`

```json
[
  {
    "id": "67ce0abe3ca0f69f633899a3",
    "conversationId": "67ce0ab53ca0f69f633899a2",
    "senderUserId": "user-1",
    "recipientUserId": "user-2",
    "content": "Recipe video here",
    "attachments": [
      {
        "type": 1,
        "url": "https://cdn.example.com/video.mp4",
        "title": "How to make pasta",
        "thumbnailUrl": "https://cdn.example.com/video-thumb.jpg"
      }
    ],
    "sentAt": "2026-02-07T18:25:04.000Z",
    "isRead": false
  }
]
```

### 3) Create messaging request

- **POST** `/api/Messaging/requests`
- **Body**

```json
{
  "recipientUserId": "user-2"
}
```

### 4) Respond to messaging request

- **POST** `/api/Messaging/requests/{requestId}/respond`
- **Body**

```json
{
  "accept": true
}
```

### 5) Send chat message (supports multimedia)

- **POST** `/api/Messaging/messages`
- **Body**

```json
{
  "conversationId": "67ce0ab53ca0f69f633899a2",
  "recipientUserId": "user-2",
  "content": "Here is the recipe photo + link",
  "attachments": [
    {
      "type": 0,
      "url": "https://cdn.example.com/photo.jpg",
      "title": "Dish photo",
      "thumbnailUrl": ""
    },
    {
      "type": 2,
      "url": "https://example.com/recipe",
      "title": "Recipe link",
      "thumbnailUrl": "https://cdn.example.com/link-thumb.jpg"
    }
  ]
}
```

### 6) Notifications

- **GET** `/api/Notifications?unreadOnly=false&take=50`
- **GET** `/api/Notifications/unread-count`
- **POST** `/api/Notifications/{notificationId}/read`

---

## Enum values used by frontend

### `MediaAttachmentType`

- `0` = `Photo`
- `1` = `Video`
- `2` = `Link`

### `MessageRequestStatus`

- `0` = `Pending`
- `1` = `Accepted`
- `2` = `Rejected`

### `NotificationType`

- `0` = `MessageRequest`
- `1` = `Message`
- `2` = `Like`
- `3` = `Action`

---

## SignalR hub integration

- Hub endpoint: `/hubs/messaging`
- Auth: same JWT (Bearer). For JS client, use `accessTokenFactory`.

### Client -> server methods

- `Handshake()`
- `SendMessageRequest(CreateMessageRequestModel model)`
- `RespondToMessageRequest(string requestId, RespondMessageRequestModel model)`
- `SendMessage(SendMessageModel model)`

### Server -> client events

- `HandshakeAcknowledged(MessagingHandshake handshake)`
- `MessageRequestReceived(MessageRequest request)`
- `MessageRequestUpdated(MessageRequest request)`
- `MessageReceived(ChatMessage message)`
- `NotificationReceived(Notification notification)`

### Handshake payload

```json
{
  "userId": "user-1",
  "connectionId": "YvP0r7k6O9v8Q8R0W4eY5Q",
  "serverTimeUtc": "2026-02-07T18:23:22.000Z",
  "pendingRequestCount": 1,
  "unreadNotificationCount": 4
}
```

### Minimal JavaScript client example

```ts
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://<api-host>/hubs/messaging", {
    accessTokenFactory: () => jwtToken
  })
  .withAutomaticReconnect()
  .build();

connection.on("HandshakeAcknowledged", (handshake) => {
  console.log("connected", handshake);
});

connection.on("MessageReceived", (message) => {
  console.log("new message", message);
});

connection.on("NotificationReceived", (notification) => {
  console.log("notification", notification);
});

await connection.start();
await connection.invoke("Handshake");
```
