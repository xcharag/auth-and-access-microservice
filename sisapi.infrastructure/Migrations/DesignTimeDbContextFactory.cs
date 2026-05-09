using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using sisapi.infrastructure.Context.Core;

namespace sisapi.infrastructure.Migrations;

/// <summary>
/// Provides a CoreDbContext instance at design-time so that the dotnet-ef CLI
/// can generate migrations without needing a running application.
///
/// Usage — pass the provider name as the first argument after "--":
///
///   # Add a PostgreSQL migration:
///   dotnet ef migrations add MyFeature \
///     --project sisapi.infrastructure \
///     --startup-project sisapi \
///     --output-dir Migrations/PostgreSQL \
///     -- PostgreSQL
///
///   # Add a SQL Server migration:
///   dotnet ef migrations add MyFeature \
///     --project sisapi.infrastructure \
///     --startup-project sisapi \
///     --output-dir Migrations/SqlServer \
///     -- SqlServer
///
///   # Remove last SQL Server migration (if not yet applied):
///   dotnet ef migrations remove \
///     --project sisapi.infrastructure \
///     --startup-project sisapi \
///     -- SqlServer
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<CoreDbContext>
{
    public CoreDbContext CreateDbContext(string[] args)
    {
        var provider = args.FirstOrDefault() ?? "SqlServer";
        var optionsBuilder = new DbContextOptionsBuilder<CoreDbContext>();

        if (provider == "PostgreSQL")
        {
            AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);
            optionsBuilder.UseNpgsql(
                // Design-time connection — actual value not used when generating migrations
                "Host=localhost;Port=5432;Database=sisapi_design;Username=sisapi;Password=LocalDev123$",
                x => x
                    .MigrationsAssembly("sisapi.infrastructure")
                    .EnableRetryOnFailure(3))
                .ReplaceService<IMigrationsAssembly, ProviderMigrationsAssembly>();
        }
        else
        {
            optionsBuilder.UseSqlServer(
                // Design-time connection — actual value not used when generating migrations
                "Server=localhost,1433;Database=sisapi_design;User Id=sa;Password=Local123$;TrustServerCertificate=True;",
                x => x
                    .MigrationsAssembly("sisapi.infrastructure")
                    .EnableRetryOnFailure(3))
                .ReplaceService<IMigrationsAssembly, ProviderMigrationsAssembly>();
        }

        return new CoreDbContext(optionsBuilder.Options);
    }
}

