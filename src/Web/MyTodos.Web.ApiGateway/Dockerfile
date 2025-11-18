# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_VERSION=1.0.0
ARG BUILD_SHA=local
WORKDIR /src

# Copy solution file
COPY MyTodos.sln .

# Copy API Gateway project
COPY src/Web/MyTodos.Web.ApiGateway/MyTodos.Web.ApiGateway.csproj src/Web/MyTodos.Web.ApiGateway/

# Restore dependencies
RUN dotnet restore src/Web/MyTodos.Web.ApiGateway/MyTodos.Web.ApiGateway.csproj

# Copy all source code
COPY src/Web/MyTodos.Web.ApiGateway/ src/Web/MyTodos.Web.ApiGateway/

# Build and publish
WORKDIR /src/src/Web/MyTodos.Web.ApiGateway
RUN dotnet publish -c Release -o /app/publish \
    /p:Version=${BUILD_VERSION} \
    /p:InformationalVersion=${BUILD_SHA} \
    --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published app
COPY --from=build /app/publish .

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV BUILD_VERSION=${BUILD_VERSION:-1.0.0}
ENV BUILD_SHA=${BUILD_SHA:-local}

ENTRYPOINT ["dotnet", "MyTodos.Web.ApiGateway.dll"]
