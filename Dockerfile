# Multi-service Dockerfile (root-level)
# Example: Build all microservices in one image (not recommended for prod)
# For individual service Dockerfiles, see each service directory.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "src/UserService/UserService.csproj"
RUN dotnet restore "src/ProductService/ProductService.csproj"
RUN dotnet restore "src/OrderService/OrderService.csproj"
RUN dotnet restore "src/AdminService/AdminService.csproj"
RUN dotnet restore "src/SearchService/SearchService.csproj"
RUN dotnet restore "src/NotificationService/NotificationService.csproj"
RUN dotnet restore "src/ApiGateway/ApiGateway.csproj"
RUN dotnet build "src/UserService/UserService.csproj" -c Release -o /app/build/UserService
RUN dotnet build "src/ProductService/ProductService.csproj" -c Release -o /app/build/ProductService
RUN dotnet build "src/OrderService/OrderService.csproj" -c Release -o /app/build/OrderService
RUN dotnet build "src/AdminService/AdminService.csproj" -c Release -o /app/build/AdminService
RUN dotnet build "src/SearchService/SearchService.csproj" -c Release -o /app/build/SearchService
RUN dotnet build "src/NotificationService/NotificationService.csproj" -c Release -o /app/build/NotificationService
RUN dotnet build "src/ApiGateway/ApiGateway.csproj" -c Release -o /app/build/ApiGateway

FROM build AS publish
RUN dotnet publish "src/UserService/UserService.csproj" -c Release -o /app/publish/UserService
RUN dotnet publish "src/ProductService/ProductService.csproj" -c Release -o /app/publish/ProductService
RUN dotnet publish "src/OrderService/OrderService.csproj" -c Release -o /app/publish/OrderService
RUN dotnet publish "src/AdminService/AdminService.csproj" -c Release -o /app/publish/AdminService
RUN dotnet publish "src/SearchService/SearchService.csproj" -c Release -o /app/publish/SearchService
RUN dotnet publish "src/NotificationService/NotificationService.csproj" -c Release -o /app/publish/NotificationService
RUN dotnet publish "src/ApiGateway/ApiGateway.csproj" -c Release -o /app/publish/ApiGateway

# Example: Only copy NotificationService for Notification container
FROM base AS notificationservice
WORKDIR /app
COPY --from=publish /app/publish/NotificationService .
ENTRYPOINT ["dotnet", "NotificationService.dll"]
