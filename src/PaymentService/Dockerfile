FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["PaymentService.csproj", "./"]
RUN dotnet restore "./PaymentService.csproj"
COPY . .
WORKDIR "/src/"
RUN dotnet build "PaymentService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PaymentService.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
# Install curl and Stripe CLI for local development
RUN apt-get update && \
	apt-get install -y curl && \
	curl -fsSL https://stripe.com/install.sh | sh
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PaymentService.dll"]
