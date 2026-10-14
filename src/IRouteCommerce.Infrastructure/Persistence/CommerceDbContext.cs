using IRouteCommerce.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace IRouteCommerce.Infrastructure.Persistence;

/// <summary>
/// DbContext de EF Core. Mapea las entidades a las tablas creadas por
/// scripts/init-db.sql (commerce, commerce_quarantine, app_users).
/// </summary>
public class CommerceDbContext : DbContext
{
    public CommerceDbContext(DbContextOptions<CommerceDbContext> options)
        : base(options)
    {
    }

    public DbSet<Commerce> Commerces => Set<Commerce>();
    public DbSet<CommerceQuarantine> CommerceQuarantines => Set<CommerceQuarantine>();
    public DbSet<AppUser> AppUsers => Set<AppUser>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Commerce>(entity =>
        {
            entity.ToTable("commerce", "dbo");
            entity.HasKey(e => e.PcId);
            entity.Property(e => e.PcId).HasColumnName("pc_id").ValueGeneratedNever();
            entity.Property(e => e.PcNomcomred).HasColumnName("pc_nomcomred").HasMaxLength(200);
            entity.Property(e => e.PcNumdoc).HasColumnName("pc_numdoc").HasMaxLength(20);
            entity.Property(e => e.PcTipdoc).HasColumnName("pc_tipdoc").HasMaxLength(10);
            entity.Property(e => e.PcDireccion).HasColumnName("pc_direccion").HasMaxLength(250);
            entity.Property(e => e.PcDepartamento).HasColumnName("pc_departamento").HasMaxLength(100);
            entity.Property(e => e.PcProvincia).HasColumnName("pc_provincia").HasMaxLength(100);
            entity.Property(e => e.PcDistrito).HasColumnName("pc_distrito").HasMaxLength(100);
            entity.Property(e => e.PcTelefono).HasColumnName("pc_telefono").HasMaxLength(30);
            entity.Property(e => e.PcEmail).HasColumnName("pc_email").HasMaxLength(150);
            entity.Property(e => e.PcProcessdate).HasColumnName("pc_processdate");
        });

        modelBuilder.Entity<CommerceQuarantine>(entity =>
        {
            entity.ToTable("commerce_quarantine", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PcId).HasColumnName("pc_id");
            entity.Property(e => e.PcNomcomred).HasColumnName("pc_nomcomred").HasMaxLength(200);
            entity.Property(e => e.PcNumdoc).HasColumnName("pc_numdoc").HasMaxLength(20);
            entity.Property(e => e.PcTipdoc).HasColumnName("pc_tipdoc").HasMaxLength(10);
            entity.Property(e => e.PcDireccion).HasColumnName("pc_direccion").HasMaxLength(250);
            entity.Property(e => e.PcDepartamento).HasColumnName("pc_departamento").HasMaxLength(100);
            entity.Property(e => e.PcProvincia).HasColumnName("pc_provincia").HasMaxLength(100);
            entity.Property(e => e.PcDistrito).HasColumnName("pc_distrito").HasMaxLength(100);
            entity.Property(e => e.PcTelefono).HasColumnName("pc_telefono").HasMaxLength(30);
            entity.Property(e => e.PcEmail).HasColumnName("pc_email").HasMaxLength(150);
            entity.Property(e => e.PcProcessdate).HasColumnName("pc_processdate");
            entity.Property(e => e.Motivo).HasColumnName("motivo").HasMaxLength(300).IsRequired();
            entity.Property(e => e.FechaCuarentena).HasColumnName("fecha_cuarentena");
        });

        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.ToTable("app_users", "dbo");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Username).HasColumnName("username").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").HasMaxLength(500).IsRequired();
            entity.Property(e => e.Role).HasColumnName("role").HasMaxLength(30).IsRequired();
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
        });
    }
}
