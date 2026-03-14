using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using sisapi.domain.Entities;

namespace sisapi.infrastructure.Context.Core;

public class CoreDbContext : IdentityDbContext<User, Role, int, IdentityUserClaim<int>,
    UserRole, IdentityUserLogin<int>, IdentityRoleClaim<int>, IdentityUserToken<int>>
{
    public CoreDbContext(DbContextOptions<CoreDbContext> options) : base(options)
    {
    }

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Company> Companies { get; set; }
    public DbSet<InterestedUser> InterestedUsers { get; set; }
    public DbSet<Person> Persons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Identity tables with custom names
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.Property(e => e.DeletedBy).HasMaxLength(50);

            entity.HasOne(u => u.Company)
                .WithMany(c => c.Users)
                .HasForeignKey(u => u.CompanyId)
                .OnDelete(DeleteBehavior.SetNull);

            // Configure one-to-one relationship with Person
            // Person is the dependent side with UserId as foreign key
            entity.HasOne(u => u.Person)
                .WithOne(p => p.User)
                .HasForeignKey<Person>(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Company
        modelBuilder.Entity<Company>(entity =>
        {
            entity.ToTable("Companies");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(e => e.Nit).HasMaxLength(50);
            entity.Property(e => e.Address).HasMaxLength(500);
            entity.Property(e => e.City).HasMaxLength(100);
            entity.Property(e => e.State).HasMaxLength(100);
            entity.Property(e => e.Country).HasMaxLength(100);
            entity.Property(e => e.PostalCode).HasMaxLength(20);
            entity.Property(e => e.Phone).HasMaxLength(30);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.Property(e => e.Website).HasMaxLength(200);
            entity.Property(e => e.LogoUrl).HasMaxLength(500);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => e.Name);
            entity.HasIndex(e => e.Nit);
        });

        // Configure Person
        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Persons");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(150);

            entity.Property(e => e.SecondName).HasMaxLength(150);
            entity.Property(e => e.LastName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.SurName).HasMaxLength(150);
            entity.Property(e => e.TypeDocument).IsRequired().HasConversion<int>();
            entity.Property(e => e.DocumentNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.DocumentException).HasMaxLength(10);
            entity.Property(e => e.DocumentExpedition).HasMaxLength(10);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Gender).IsRequired().HasConversion<int>();

            entity.HasIndex(e => e.DocumentNumber);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Roles");
            entity.Property(e => e.Description).HasMaxLength(200);
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("UserRoles");
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId);

            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
        });
        
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(e => e.Id);
    
            entity.Property(e => e.Token).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Token).IsUnique();
    
            entity.HasOne(rt => rt.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(rt => rt.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        
            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
        });

        modelBuilder.Entity<IdentityUserClaim<int>>().ToTable("UserClaims");
        modelBuilder.Entity<IdentityUserLogin<int>>().ToTable("UserLogins");
        modelBuilder.Entity<IdentityRoleClaim<int>>().ToTable("RoleClaims");
        modelBuilder.Entity<IdentityUserToken<int>>().ToTable("UserTokens");

        // Configure Permission
        modelBuilder.Entity<Permission>(entity =>
        {
            entity.ToTable("Permissions");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(e => e.Description)
                .HasMaxLength(250);

            entity.Property(e => e.Module)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.TypePermission)
                .IsRequired()
                .HasConversion<int>();

            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => new { e.Code, e.Module, e.TypePermission }).IsUnique();
            entity.HasIndex(e => e.Module);
        });

        // Configure RolePermission
        modelBuilder.Entity<RolePermission>(entity =>
        {
            entity.ToTable("RolePermissions");
            entity.HasKey(e => e.Id);

            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(rp => rp.Company)
                .WithMany(c => c.RolePermissions)
                .HasForeignKey(rp => rp.CompanyId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // CompanyId is nullable - allows global permissions

            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);
            entity.HasIndex(rp => new { rp.CompanyId, rp.RoleId, rp.PermissionId }).IsUnique();
        });

        // Configure InterestedUser
        modelBuilder.Entity<InterestedUser>(entity =>
        {
            entity.ToTable("InterestedUsers");
            entity.HasKey(e => e.Id);

            entity.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(256);

            entity.Property(e => e.Password)
                .IsRequired()
                .HasMaxLength(500);

            entity.Property(e => e.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.LastName)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(e => e.PhoneNumber)
                .IsRequired()
                .HasMaxLength(30);

            entity.Property(e => e.CreatedBy).HasMaxLength(50);
            entity.Property(e => e.UpdatedBy).HasMaxLength(50);

            entity.HasIndex(e => e.Email).IsUnique();

        });
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries()
            .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseEntity baseEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    baseEntity.CreatedAt = DateTime.UtcNow;
                    // TODO: Get current user from HttpContext
                    // baseEntity.CreatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                }
                else if (entry.State == EntityState.Modified)
                {
                    baseEntity.UpdatedAt = DateTime.UtcNow;
                    // TODO: Get current user from HttpContext
                    // baseEntity.UpdatedBy = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
                }
            }
            // Handle Identity entities with audit fields
            else if (entry.Entity is User user)
            {
                if (entry.State == EntityState.Modified)
                {
                    user.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is Role role)
            {
                if (entry.State == EntityState.Modified)
                {
                    role.UpdatedAt = DateTime.UtcNow;
                }
            }
            else if (entry.Entity is UserRole userRole)
            {
                if (entry.State == EntityState.Modified)
                {
                    userRole.UpdatedAt = DateTime.UtcNow;
                }
            }
        }
    }
}

