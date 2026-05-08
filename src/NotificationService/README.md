# NotificationService

A microservice for handling notifications (Email, SMS, Push) for MCart. Integrates with Azure Service Bus for event-driven notifications.

## Features
- Send Email, SMS, and Push notifications
- Store notification status/history in SQL Server
- Consume notification events from Azure Service Bus
- Ready for Docker and Kubernetes deployment

## Endpoints
- `POST /api/notifications/email` — Send email notification
- `POST /api/notifications/sms` — Send SMS notification
- `POST /api/notifications/push` — Send push notification
- `GET /api/notifications` — List all notifications

## Azure Service Bus
- Consumes messages from the `notifications` queue
- Add your Service Bus connection string and queue name in `appsettings.Development.json`

## Running Locally
1. Update `appsettings.json` with your SQL Server connection string
2. Update `appsettings.Development.json` with your Service Bus details
3. Build and run:
   ```sh
   dotnet build
   dotnet run
   ```

## Docker
Build and run the container:
```sh
docker build -t notificationservice .
docker run -p 8080:80 notificationservice
```

## Kubernetes
Apply the manifest:
```sh
kubectl apply -f k8s-notification.yaml
```

## TODO
- Integrate with SendGrid/Azure Communication Services for email/SMS
- Integrate with Azure Notification Hubs for push
- Add retry and dead-letter handling
- Add authentication/authorization
