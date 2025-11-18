# Build stage
FROM node:20-alpine AS build
ARG BUILD_VERSION=1.0.0
ARG BUILD_SHA=local
WORKDIR /app

# Copy package files
COPY src/Web/MyTodos.Web.ClientApp/package*.json ./

# Install dependencies
RUN npm ci

# Copy source code
COPY src/Web/MyTodos.Web.ClientApp/ ./

# Build the application
RUN npm run build

# Runtime stage
FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html

# Remove default nginx static assets
RUN rm -rf ./*

# Copy built app from build stage
COPY --from=build /app/dist .

# Copy nginx configuration
COPY src/Web/MyTodos.Web.ClientApp/nginx.conf /etc/nginx/conf.d/default.conf

# Expose port
EXPOSE 80

# Set environment variables
ENV BUILD_VERSION=${BUILD_VERSION:-1.0.0}
ENV BUILD_SHA=${BUILD_SHA:-local}

CMD ["nginx", "-g", "daemon off;"]
