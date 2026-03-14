using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using sisapi.domain.Entities;
using Microsoft.OpenApi.Models;
using sisapi.infrastructure.Authorization;
using sisapi.infrastructure.Context.Core;
using sisapi.infrastructure.Services;

var builder = WebApplication.CreateBuilder(args);
var cookieDomain = builder.Configuration["CookieDomain"] ?? ".xchar.site";
var cookieName = builder.Configuration["Authentication:CookieName"] ?? "SisApi.Auth";

builder.Services.AddDbContext<CoreDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CoreConnection"),
        sqlServerOptions => sqlServerOptions
            .EnableRetryOnFailure(
                maxRetryCount: 10,
                maxRetryDelay: TimeSpan.FromSeconds(30),
                errorNumbersToAdd: null)
            .CommandTimeout(60)));

builder.Services.AddIdentity<User, Role>(options =>
    {
        options.Password.RequireDigit = true;
        options.Password.RequireLowercase = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Password.RequiredLength = 8;

        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.AllowedForNewUsers = true;

        options.User.RequireUniqueEmail = true;
    })
    .AddEntityFrameworkStores<CoreDbContext>()
    .AddDefaultTokenProviders();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");

builder.Services.AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings["Issuer"],
            ValidAudience = jwtSettings["Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrWhiteSpace(token))
                {
                    token = context.Request.Cookies["accessToken"];
                    if (!string.IsNullOrWhiteSpace(token))
                    {
                        context.Token = token;
                        Console.WriteLine("JWT Token read from cookie");
                    }
                    else
                    {
                        Console.WriteLine("JWT Token: NO TOKEN");
                    }
                }
                else
                {
                    context.Token = token;
                    Console.WriteLine("JWT Token read from Authorization header");
                }

                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine($"JWT Authentication Failed: {context.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = _ =>
            {
                Console.WriteLine("JWT Token Validated Successfully");
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Console.WriteLine($"JWT Challenge: {context.Error}, {context.ErrorDescription}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = cookieName;
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.Path = "/";
    if (!string.IsNullOrWhiteSpace(cookieDomain))
    {
        options.Cookie.Domain = cookieDomain;
    }

    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context =>
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        return Task.CompletedTask;
    };
});

builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddAuthorization();

var allowedOrigins = builder.Configuration["AllowedOrigins"]?.Split(',', StringSplitOptions.RemoveEmptyEntries) 
                     ?? new[] { "https://localhost:5173" };

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("CORS Configuration:");
Console.WriteLine($"Environment: {builder.Environment.EnvironmentName}");
Console.WriteLine($"Allowed Origins ({allowedOrigins.Length}):");
foreach (var origin in allowedOrigins)
{
    Console.WriteLine($"  - {origin}");
}
Console.WriteLine($"Cookie Domain: {cookieDomain}");

Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine("JasperServer Configuration:");
Console.WriteLine($"  URL: {builder.Configuration["JasperServer:Url"]}");
Console.WriteLine($"  Username: {builder.Configuration["JasperServer:Username"]}");
Console.WriteLine("=".PadRight(60, '='));

builder.Services.AddCors(options =>
{
    options.AddPolicy("ProductionPolicy", policy =>
    {
        policy.WithOrigins(allowedOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
    
    options.AddPolicy("FrontendDev", policy =>
    {
        policy.WithOrigins("https://localhost:5173", "http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

builder.Services.AddHealthChecks()
    .AddSqlServer(
        connectionString: builder.Configuration.GetConnectionString("CoreConnection") ?? "",
        name: "database",
        tags: new[] { "db", "sql", "sqlserver" }
    );

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

builder.Services.AddScoped<sisapi.application.Contracts.IJwtService, sisapi.application.Implementations.JwtService>();
builder.Services.AddScoped<sisapi.application.Contracts.IAuthService, sisapi.application.Implementations.AuthService>();
builder.Services.AddScoped<sisapi.application.Contracts.IRoleService, sisapi.application.Implementations.RoleService>();
builder.Services.AddScoped<sisapi.application.Contracts.IPermissionService, sisapi.application.Implementations.PermissionService>();
builder.Services.AddScoped<sisapi.application.Contracts.IRolePermissionService, sisapi.application.Implementations.RolePermissionService>();
builder.Services.AddScoped<sisapi.application.Contracts.ICompanyService, sisapi.application.Implementations.CompanyService>();
builder.Services.AddScoped<sisapi.application.Contracts.IUserService, sisapi.application.Implementations.UserService>();
builder.Services.AddScoped<sisapi.application.Contracts.IInterestedUserService, sisapi.application.Implementations.InterestedUserService>();
builder.Services.AddScoped<sisapi.domain.Abstractions.IPermissionVerifier, sisapi.application.Implementations.PermissionVerifier>();
builder.Services.AddScoped<sisapi.application.Contracts.IProjectPermissionService, sisapi.application.Implementations.ProjectPermissionService>();

builder.Services.AddScoped<IPasswordHasher<InterestedUser>, PasswordHasher<InterestedUser>>();

builder.Services.AddScoped<DatabaseSeeder>();

builder.Services.AddScoped<sisapi.application.Services.Reports.IExcelReportBuilder, sisapi.application.Services.Reports.ExcelReportBuilder>();
builder.Services.AddScoped<sisapi.application.Services.Reports.Strategies.IReportStrategyFactory, sisapi.application.Services.Reports.Strategies.ReportStrategyFactory>();
builder.Services.AddScoped<sisapi.application.Services.Reports.Strategies.UserReportStrategy>();
builder.Services.AddScoped<sisapi.application.Services.Reports.Strategies.RoleReportStrategy>();
builder.Services.AddScoped<sisapi.application.Services.Reports.Strategies.CompanyReportStrategy>();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo { Title = "SisApi", Version = "v1" });

   options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter your JWT token in the text input below.",
    });
    
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

var corsPolicy = app.Environment.IsDevelopment() ? "FrontendDev" : "ProductionPolicy";
Console.WriteLine("=".PadRight(60, '='));
Console.WriteLine($"Active CORS Policy: {corsPolicy}");
Console.WriteLine("=".PadRight(60, '='));
app.UseCors(corsPolicy);

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    
    try
    {
        var context = services.GetRequiredService<CoreDbContext>();
        
        logger.LogInformation("Applying database migrations...");
        await context.Database.MigrateAsync();
        logger.LogInformation("Database migrations applied successfully.");
        
        var seeder = services.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
        logger.LogInformation("Database seeding completed successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
        throw;
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "SisApi v1");
    c.RoutePrefix = "swagger"; 
});

app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .AllowAnonymous();

if (!app.Environment.IsProduction())
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");

app.MapControllers();

app.Run();

