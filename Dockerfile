# Use the official .NET SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution file
COPY sisapi.sln ./

# Copy all project files
COPY sisapi/sisapi.csproj sisapi/
COPY sisapi.application/sisapi.application.csproj sisapi.application/
COPY sisapi.domain/sisapi.domain.csproj sisapi.domain/
COPY sisapi.infrastructure/sisapi.infrastructure.csproj sisapi.infrastructure/

# Restore dependencies
RUN dotnet restore sisapi.sln

# Copy all source files
COPY sisapi/ sisapi/
COPY sisapi.application/ sisapi.application/
COPY sisapi.domain/ sisapi.domain/
COPY sisapi.infrastructure/ sisapi.infrastructure/

# Build the application
WORKDIR /src/sisapi
RUN dotnet build -c Release -o /app/build

# Publish the application
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use the official .NET runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Install curl for health checks (optional but recommended)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=build /app/publish .

# Create a non-root user for security
RUN useradd -m -u 1001 appuser && chown -R appuser:appuser /app
USER appuser

# Expose port
EXPOSE 8080

# Set environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=40s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "sisapi.dll"]
