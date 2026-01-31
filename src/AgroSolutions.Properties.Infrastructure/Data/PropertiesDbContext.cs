using AgroSolutions.Properties.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AgroSolutions.Properties.Infrastructure.Data;

public class PropertiesDbContext(DbContextOptions<PropertiesDbContext> options) : DbContext(options)
{
    public DbSet<Produtor> Produtores => Set<Produtor>();
    public DbSet<Fazenda> Fazendas => Set<Fazenda>();
    public DbSet<Talhao> Talhoes => Set<Talhao>();
    public DbSet<Sensor> Sensores => Set<Sensor>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações de Produtor
        modelBuilder.Entity<Produtor>(entity =>
        {
            entity.ToTable("Produtores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Cpf).IsRequired().HasMaxLength(11);
            entity.HasIndex(e => e.Cpf).IsUnique();
            entity.Property(e => e.Email).HasMaxLength(200);
            entity.Property(e => e.Telefone).HasMaxLength(20);

            entity
                .HasMany(e => e.Fazendas)
                .WithOne(f => f.Produtor)
                .HasForeignKey(f => f.ProdutorId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações de Fazenda
        modelBuilder.Entity<Fazenda>(entity =>
        {
            entity.ToTable("Fazendas");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.AreaTotal).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");

            entity
                .HasMany(e => e.Talhoes)
                .WithOne(t => t.Fazenda)
                .HasForeignKey(t => t.FazendaId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações de Talhao
        modelBuilder.Entity<Talhao>(entity =>
        {
            entity.ToTable("Talhoes");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Nome).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Area).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Cultura).HasMaxLength(100);
            entity.Property(e => e.Status).HasConversion<string>();

            entity
                .HasMany(e => e.Sensores)
                .WithOne(s => s.Talhao)
                .HasForeignKey(s => s.TalhaoId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configurações de Sensor
        modelBuilder.Entity<Sensor>(entity =>
        {
            entity.ToTable("Sensores");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.CodigoIdentificacao).IsRequired().HasMaxLength(50);
            entity.HasIndex(e => e.CodigoIdentificacao).IsUnique();
            entity.Property(e => e.Tipo).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.Modelo).HasMaxLength(100);
            entity.Property(e => e.Fabricante).HasMaxLength(100);
            entity.Property(e => e.Latitude).HasColumnType("decimal(10,8)");
            entity.Property(e => e.Longitude).HasColumnType("decimal(11,8)");
        });
    }
}
