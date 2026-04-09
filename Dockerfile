# Stage 1: Build .NET backend
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
WORKDIR /src
COPY backend/ ./
RUN dotnet restore Pulse.Api.csproj
RUN dotnet publish Pulse.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Build Vue frontend
FROM node:22-alpine AS frontend-build
WORKDIR /frontend
COPY frontend/package*.json ./
RUN npm ci
COPY frontend/ ./
RUN npm run build

# Stage 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0

RUN apt-get update && apt-get install -y --no-install-recommends \
    nginx \
    supervisor \
    && rm -rf /var/lib/apt/lists/*

COPY --from=backend-build /app/publish /app
COPY --from=frontend-build /frontend/dist /usr/share/nginx/html
COPY nginx/nginx.conf /etc/nginx/nginx.conf
COPY supervisord.conf /etc/supervisor/conf.d/supervisord.conf

RUN mkdir -p /data/covers /media /var/log/nginx /var/log \
    && chmod -R 755 /data /media

VOLUME ["/media", "/data"]

ENV MUSIC_PATH=/media \
    DATA_PATH=/data \
    ASPNETCORE_URLS=http://localhost:5000

EXPOSE 80

CMD ["/usr/bin/supervisord", "-c", "/etc/supervisor/conf.d/supervisord.conf"]
