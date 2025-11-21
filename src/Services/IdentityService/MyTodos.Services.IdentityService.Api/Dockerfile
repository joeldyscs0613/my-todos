# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_VERSION=1.0.0
ARG BUILD_SHA=local
WORKDIR /src

# Copy solution file
COPY MyTodos.sln .

# Copy SharedKernel project
COPY src/MyTodos.SharedKernel/MyTodos.SharedKernel.csproj src/MyTodos.SharedKernel/

# Copy BuildingBlocks projects
COPY src/BuildingBlocks/MyTodos.BuildingBlocks.Application/MyTodos.BuildingBlocks.Application.csproj src/BuildingBlocks/MyTodos.BuildingBlocks.Application/
COPY src/BuildingBlocks/MyTodos.BuildingBlocks.Infrastructure/MyTodos.BuildingBlocks.Infrastructure.csproj src/BuildingBlocks/MyTodos.BuildingBlocks.Infrastructure/
COPY src/BuildingBlocks/MyTodos.BuildingBlocks.Presentation/MyTodos.BuildingBlocks.Presentation.csproj src/BuildingBlocks/MyTodos.BuildingBlocks.Presentation/

# Copy Contracts projects
COPY src/Services/IdentityService/MyTodos.Services.IdentityService.Contracts/MyTodos.Services.IdentityService.Contracts.csproj src/Services/IdentityService/MyTodos.Services.IdentityService.Contracts/
COPY src/Services/TodoService/MyTodos.Services.TodoService.Contracts/MyTodos.Services.TodoService.Contracts.csproj src/Services/TodoService/MyTodos.Services.TodoService.Contracts/
COPY src/Services/NotificationService/MyTodos.Services.NotificationService.Contracts/MyTodos.Services.NotificationService.Contracts.csproj src/Services/NotificationService/MyTodos.Services.NotificationService.Contracts/

# Copy IdentityService projects
COPY src/Services/IdentityService/MyTodos.Services.IdentityService.Domain/MyTodos.Services.IdentityService.Domain.csproj src/Services/IdentityService/MyTodos.Services.IdentityService.Domain/
COPY src/Services/IdentityService/MyTodos.Services.IdentityService.Application/MyTodos.Services.IdentityService.Application.csproj src/Services/IdentityService/MyTodos.Services.IdentityService.Application/
COPY src/Services/IdentityService/MyTodos.Services.IdentityService.Infrastructure/MyTodos.Services.IdentityService.Infrastructure.csproj src/Services/IdentityService/MyTodos.Services.IdentityService.Infrastructure/
COPY src/Services/IdentityService/MyTodos.Services.IdentityService.Api/MyTodos.Services.IdentityService.Api.csproj src/Services/IdentityService/MyTodos.Services.IdentityService.Api/

# Restore dependencies
RUN dotnet restore src/MyTodos.SharedKernel/MyTodos.SharedKernel.csproj && \
    dotnet restore src/Services/IdentityService/MyTodos.Services.IdentityService.Api/MyTodos.Services.IdentityService.Api.csproj

# Copy all source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/MyTodos.SharedKernel/ src/MyTodos.SharedKernel/
COPY src/Services/IdentityService/ src/Services/IdentityService/
COPY src/Services/TodoService/MyTodos.Services.TodoService.Contracts/ src/Services/TodoService/MyTodos.Services.TodoService.Contracts/
COPY src/Services/NotificationService/MyTodos.Services.NotificationService.Contracts/ src/Services/NotificationService/MyTodos.Services.NotificationService.Contracts/

# Build and publish
WORKDIR /src/src/Services/IdentityService/MyTodos.Services.IdentityService.Api
RUN dotnet publish -c Release -o /app/publish \
    /p:Version=${BUILD_VERSION} \
    /p:InformationalVersion=${BUILD_SHA} \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published app
COPY --from=build /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Expose port
EXPOSE 5001

# Set environment variables
ENV ASPNETCORE_URLS=http://+:5001
ENV BUILD_VERSION=${BUILD_VERSION:-1.0.0}
ENV BUILD_SHA=${BUILD_SHA:-local}

ENTRYPOINT ["dotnet", "MyTodos.Services.IdentityService.Api.dll"]
