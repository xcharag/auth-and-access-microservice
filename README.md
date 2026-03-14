# SisApi - Docker & Production Deployment

Complete authentication and access control API built with .NET 9, ready for production deployment with Docker and Coolify.

## 🚀 Quick Start

### Local Development

```bash
# Clone the repository
git clone https://github.com/yourusername/sisapi.git
cd sisapi

# Restore dependencies
dotnet restore

# Run the application
dotnet run --project sisapi

# Or using Docker
docker-compose up
```

### Production Deployment with Coolify

See the comprehensive guides:
- 🚀 **[Complete Coolify Deployment Guide](docs/Deploy/COOLIFY_COMPLETE_DEPLOYMENT_GUIDE.md)** - Full stack deployment with SQL Server & JasperServer
- 📘 **[Production Deployment Guide](docs/Deploy/PRODUCTION_DEPLOYMENT_GUIDE.md)** - Complete production setup
- ⚡ **[Coolify Quick Setup](docs/Deploy/COOLIFY_QUICK_SETUP.md)** - Step-by-step Coolify configuration

### Docker Compose Files

| File | Use Case |
|------|----------|
| `docker-compose.yml` | Local development (full stack) |
| `docker-compose.coolify.yml` | Coolify deployment (full stack) |
| `docker-compose.api-only.yml` | API only (external SQL Server & JasperServer) |

## 📦 What's Included

### Features
- ✅ JWT Authentication with refresh tokens
- ✅ Role-based access control (RBAC)
- ✅ Dynamic permission system
- ✅ Cookie-based authentication
- ✅ User management
- ✅ Company/Organization support
- ✅ Interested users system
- ✅ Excel report generation
- ✅ JasperReports integration
- ✅ Health checks
- ✅ Swagger documentation

### Architecture
- **Clean Architecture** with layered design:
  - `sisapi` - API/Presentation layer
  - `sisapi.application` - Business logic
  - `sisapi.domain` - Entities and DTOs
  - `sisapi.infrastructure` - Data access and external services

### Technology Stack
- **.NET 9** - Latest .NET framework
- **Entity Framework Core** - ORM
- **SQL Server** - Database
- **JWT** - Authentication
- **Docker** - Containerization
- **Swagger/OpenAPI** - API documentation
- **ClosedXML** - Excel generation

## 🐳 Docker Configuration

### Dockerfile
Multi-stage build for optimized production images:
- Build stage: SDK 9.0 (compiles application)
- Runtime stage: ASP.NET 9.0 (runs application)
- Non-root user for security
- Health checks included

### docker-compose.yml
Local development and testing setup with:
- API container
- SQL Server container
- Volume persistence
- Network isolation

## 🔧 Configuration

### Environment Variables

Copy `.env.example` to `.env` and configure:

```bash
# Database
DB_CONNECTION_STRING=Server=db,1433;Database=sisapi;User Id=sa;Password=YourPassword;TrustServerCertificate=True

# JWT (generate with: openssl rand -base64 32)
JWT_SECRET_KEY=YourSuperSecretKeyAtLeast32Characters

# CORS
ALLOWED_ORIGINS=https://yourdomain.com

# Environment
ASPNETCORE_ENVIRONMENT=Production
```

### appsettings.json

Configuration follows .NET standards:
- `appsettings.json` - Base configuration
- `appsettings.Development.json` - Development overrides
- `appsettings.Production.json` - Production overrides (not in Git)
- **Environment variables override all** (recommended for production)

## 🏗️ Build & Deploy

### Local Build

```bash
# Build solution
dotnet build

# Run tests (if available)
dotnet test

# Publish for production
dotnet publish -c Release -o ./publish
```

### Docker Build

```bash
# Build image
docker build -t sisapi:latest .

# Run container
docker run -p 8080:8080 \
  -e ConnectionStrings__CoreConnection="Server=..." \
  -e JwtSettings__SecretKey="..." \
  sisapi:latest

# Test health endpoint
curl http://localhost:8080/health
```

### Coolify Deployment

1. **Connect repository** to Coolify
2. **Set environment variables** in Coolify UI
3. **Configure domain** and SSL
4. **Deploy** - Coolify handles everything

📘 See [COOLIFY_QUICK_SETUP.md](COOLIFY_QUICK_SETUP.md) for detailed steps.

## 📊 Health Monitoring

### Health Check Endpoint

```bash
GET /health
```

Returns:
```json
{
  "status": "Healthy",
  "results": {
    "database": {
      "status": "Healthy"
    }
  }
}
```

### Monitoring in Coolify
- Real-time logs
- Resource usage (CPU, memory)
- Automatic restarts on failure
- Alert notifications

## 🔒 Security

### Built-in Security Features
- ✅ JWT with secure secret keys
- ✅ HTTP-only cookies
- ✅ HTTPS/TLS via reverse proxy
- ✅ CORS protection
- ✅ Password hashing (ASP.NET Identity)
- ✅ Non-root container execution
- ✅ SQL injection protection (EF Core)
- ✅ Input validation

### Production Checklist
- [ ] Strong JWT secret (32+ characters)
- [ ] Strong database passwords
- [ ] HTTPS enabled
- [ ] CORS properly configured (no wildcards)
- [ ] Environment variables not in source control
- [ ] Regular security updates
- [ ] Database backups configured
- [ ] Rate limiting (consider implementing)

## 📈 Scaling

### Horizontal Scaling
The application is **stateless** and ready to scale:
- No in-memory sessions
- Database-backed authentication
- Load balancer compatible
- Multiple instances supported

### Vertical Scaling
Recommended resources:
- **Development:** 1GB RAM, 0.5 CPU
- **Production:** 2-4GB RAM, 1-2 CPU
- **High Traffic:** 4-8GB RAM, 2-4 CPU

## 🛠️ Troubleshooting

### Common Issues

#### Container won't start
```bash
# Check logs
docker logs <container-id>

# Verify environment variables
docker exec <container-id> printenv

# Test database connection
docker exec <container-id> curl http://localhost:8080/health
```

#### Database connection fails
- Verify connection string format
- Check SQL Server is running
- Ensure firewall allows connection
- Validate credentials

#### CORS errors
- Check `AllowedOrigins` setting
- Verify exact domain match (http vs https)
- No trailing slashes in URLs

See [PRODUCTION_DEPLOYMENT_GUIDE.md](PRODUCTION_DEPLOYMENT_GUIDE.md) for comprehensive troubleshooting.

## 📚 API Documentation

### Swagger UI
Access interactive API documentation:
```
https://api.yourdomain.com/swagger
```

### Endpoints

#### Authentication
- `POST /api/Auth/login` - User login
- `POST /api/Auth/refresh` - Refresh token
- `POST /api/Auth/logout` - User logout
- `GET /api/Auth/verify-permission` - Check permissions

#### Users
- `GET /api/User` - List users
- `POST /api/User` - Create user
- `PUT /api/User/{id}` - Update user
- `DELETE /api/User/{id}` - Delete user

#### Roles & Permissions
- `GET /api/Role` - List roles
- `POST /api/Role` - Create role
- `GET /api/Permission` - List permissions
- `POST /api/RolePermission` - Assign permission to role

#### Reports
- `GET /api/Report/excel` - Generate Excel report
- `GET /api/Report/jasper` - Generate JasperReport

## 🧪 Testing

### Manual Testing
Use Swagger UI or curl:
```bash
# Login
curl -X POST https://api.yourdomain.com/api/Auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"Admin123!"}'

# Use returned token
curl https://api.yourdomain.com/api/User \
  -H "Authorization: Bearer <token>"
```

### Automated Testing
(To be implemented)
```bash
# Run tests
dotnet test

# With coverage
dotnet test /p:CollectCoverage=true
```

## 🔄 Updates & Maintenance

### Application Updates
```bash
# Via Git + Coolify auto-deploy
git add .
git commit -m "Update feature"
git push origin main
# Coolify automatically deploys

# Manual in Coolify
Dashboard → Your App → Deploy
```

### Database Migrations
```bash
# Create migration
dotnet ef migrations add MigrationName --project sisapi.infrastructure

# Generate SQL script
dotnet ef migrations script --project sisapi.infrastructure -o migration.sql

# Apply automatically on startup (already configured)
```

## 💾 Backup & Restore

### Database Backup
```bash
# Using SQL Server
docker exec <sql-container> /opt/mssql-tools/bin/sqlcmd \
  -S localhost -U sa -P <password> \
  -Q "BACKUP DATABASE sisapi TO DISK='/backup/sisapi.bak'"

# Copy backup out
docker cp <sql-container>:/backup/sisapi.bak ./backup/
```

### Application Code
- Stored in Git (version controlled)
- Docker images stored in registry

## 📄 License

[Your License Here]

## 🤝 Contributing

[Your contribution guidelines]

## 📞 Support

For issues and questions:
- **Application Issues:** Check logs in Coolify
- **Deployment Issues:** See [PRODUCTION_DEPLOYMENT_GUIDE.md](PRODUCTION_DEPLOYMENT_GUIDE.md)
- **Coolify Issues:** [Coolify Documentation](https://coolify.io/docs)

## 📝 Additional Documentation

- [Production Deployment Guide](PRODUCTION_DEPLOYMENT_GUIDE.md) - Complete production setup
- [Coolify Quick Setup](COOLIFY_QUICK_SETUP.md) - Coolify configuration steps
- [Cookie Authentication Guide](COOKIE_AUTH_GUIDE.md) - Cookie-based auth implementation
- [Excel Reports Guide](docs/Excel%20Reports/EXCEL_REPORTS_GUIDE.md) - Excel report generation
- [JasperReports Guide](docs/JASPER_REPORTS_GUIDE.md) - JasperReports integration

---

**Built with ❤️ using .NET 9 and Docker**

**Last Updated:** January 27, 2026
