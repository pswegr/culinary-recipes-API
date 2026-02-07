# CulinaryRecipes.API

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
