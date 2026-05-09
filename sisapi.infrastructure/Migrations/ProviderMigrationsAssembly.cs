using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;

namespace sisapi.infrastructure.Migrations;

/// <summary>
/// Filters EF Core migrations at runtime so that only the migrations matching
/// the active provider are applied:
///   - Npgsql  → sisapi.infrastructure.Migrations.PostgreSQL.*
///   - SqlServer → sisapi.infrastructure.Migrations.SqlServer.*
///
/// Register in AddDbContext via:
///   options.UseNpgsql(...).ReplaceService&lt;IMigrationsAssembly, ProviderMigrationsAssembly&gt;()
///   options.UseSqlServer(...).ReplaceService&lt;IMigrationsAssembly, ProviderMigrationsAssembly&gt;()
/// </summary>
public class ProviderMigrationsAssembly : MigrationsAssembly
{
    private readonly string _subfolder;
    private IReadOnlyDictionary<string, TypeInfo>? _filteredMigrations;
    private ModelSnapshot? _filteredSnapshot;
    private bool _snapshotResolved;

    public ProviderMigrationsAssembly(
        ICurrentDbContext currentContext,
        IDbContextOptions options,
        IMigrationsIdGenerator idGenerator,
        IDiagnosticsLogger<DbLoggerCategory.Migrations> logger)
        : base(currentContext, options, idGenerator, logger)
    {
        var provider = currentContext.Context.Database.ProviderName ?? "";
        _subfolder = provider.Contains("Npgsql") ? "PostgreSQL" : "SqlServer";
    }

    /// <summary>Returns only the migrations whose namespace contains the active provider subfolder.</summary>
    public override IReadOnlyDictionary<string, TypeInfo> Migrations
    {
        get
        {
            _filteredMigrations ??= base.Migrations
                .Where(kvp => kvp.Value.Namespace?.Contains(_subfolder) == true)
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
            return _filteredMigrations;
        }
    }

    /// <summary>Returns the ModelSnapshot whose namespace contains the active provider subfolder.</summary>
    public override ModelSnapshot? ModelSnapshot
    {
        get
        {
            if (_snapshotResolved) return _filteredSnapshot;
            _snapshotResolved = true;

            var snapshotType = Assembly
                .DefinedTypes
                .FirstOrDefault(t =>
                    !t.IsAbstract &&
                    t.IsSubclassOf(typeof(ModelSnapshot)) &&
                    t.Namespace?.Contains(_subfolder) == true);

            _filteredSnapshot = snapshotType != null
                ? (ModelSnapshot?)Activator.CreateInstance(snapshotType)
                : null;  // Return null if no provider-specific snapshot found.
                         // This tells EF Core design-time tools to CREATE a new snapshot
                         // in the --output-dir instead of corrupting an existing one.

            return _filteredSnapshot;
        }
    }
}


