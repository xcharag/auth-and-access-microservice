# ============================================================
# Arquitectura objetivo: linux/amd64
# Si compilas desde una Mac M1/M2/M3 (ARM) esta directiva
# garantiza que la imagen resultante sea AMD64.
# Multi-stage build: la imagen final NO contiene el SDK ni
# artefactos temporales de la compilación.
# ============================================================

# -- Argumento de versión para etiquetar la imagen --
ARG VERSION=1.0.0

# ============================================================
# Etapa 1: Compilación (SDK slim — se descarta al final)
# ============================================================
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build
WORKDIR /src

# Copiar archivos de solución y proyectos para restaurar dependencias
COPY sisapi.sln ./
COPY sisapi/sisapi.csproj sisapi/
COPY sisapi.application/sisapi.application.csproj sisapi.application/
COPY sisapi.domain/sisapi.domain.csproj sisapi.domain/
COPY sisapi.infrastructure/sisapi.infrastructure.csproj sisapi.infrastructure/

# Restaurar dependencias (aprovecha la caché de Docker si los .csproj no cambian)
RUN dotnet restore sisapi.sln

# Copiar el resto del código fuente
COPY sisapi/ sisapi/
COPY sisapi.application/ sisapi.application/
COPY sisapi.domain/ sisapi.domain/
COPY sisapi.infrastructure/ sisapi.infrastructure/

# Publicar en modo Release (sin host wrapper)
WORKDIR /src/sisapi
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# ============================================================
# Etapa 2: Imagen final de ejecución (runtime slim — sin SDK)
# ============================================================
FROM --platform=linux/amd64 mcr.microsoft.com/dotnet/aspnet:9.0-bookworm-slim AS runtime

# Metadatos de versión para facilitar la identificación de la imagen desplegada
ARG VERSION
LABEL org.opencontainers.image.version="${VERSION}" \
      org.opencontainers.image.title="sisapi" \
      org.opencontainers.image.description="SisApi — Servicio de autenticación y acceso"

# Bake the version into the image as an env var so the app can read it at runtime.
# This is the single source of truth: it cannot be overridden without rebuilding the image
# (unlike a runtime env var that can be set to an incorrect value externally).
ENV APP_VERSION=${VERSION}

WORKDIR /app

# Instalar curl para el health check
RUN apt-get update && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

# Crear el directorio de logs que se monta externamente
RUN mkdir -p /app/logs

# Copiar artefactos publicados desde la etapa de compilación
COPY --from=build /app/publish .

# Usuario no-root por seguridad
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Puerto de escucha (sin SSL — el Gateway institucional se encarga del TLS)
EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=60s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "sisapi.dll"]
