
namespace ProjParkNet.Data;

//public class ParkingDbContext : IdentityDbContext
public class ParkingDbContext : IdentityDbContext<IdentityUser>
{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
        : base(options)
    {
    }

    // DbSet para as entidades
    public DbSet<Parking> Parkings { get; set; }
    public DbSet<ParkingFloor> Floors { get; set; }
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<ParkingUsage> ParkingUsages { get; set; }
    public DbSet<UserSystem> UserSystems { get; set; }
    public DbSet<UserTransaction> UserTransactions { get; set; }
    public DbSet<UserBalance> UserBalances { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuração de precisão para valores monetários
        ConfigureMonetaryFields(modelBuilder);

        // Configuração dos relacionamentos
        ConfigureRelationships(modelBuilder);

        // Configuração de índices únicos
        ConfigureUniqueIndexes(modelBuilder);
    }

    private void ConfigureMonetaryFields(ModelBuilder modelBuilder)
    {
        // Define a precisão para campos monetários
        modelBuilder.Entity<Parking>()
            .Property(p => p.MonthlyAgreement).HasPrecision(10, 2);
        modelBuilder.Entity<Parking>()
            .Property(p => p.PricePerMinute).HasPrecision(10, 2);
        modelBuilder.Entity<ParkingUsage>()
            .Property(pu => pu.Price).HasPrecision(10, 2);
        modelBuilder.Entity<UserTransaction>()
            .Property(t => t.Amount).HasPrecision(10, 2);
        modelBuilder.Entity<UserBalance>()
            .Property(b => b.CurrentBalance).HasPrecision(10, 2);
    }

    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // Relacionamento entre ParkingFloor e Parking
        modelBuilder.Entity<ParkingFloor>()
            .HasOne(pf => pf.Parking)
            .WithMany(p => p.Floors)
            .HasForeignKey(pf => pf.ParkingId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento entre ParkingSpot e ParkingFloor
        modelBuilder.Entity<ParkingSpot>()
            .HasOne(ps => ps.ParkingFloor)
            .WithMany(pf => pf.Spots)
            .HasForeignKey(ps => ps.ParkingFloorId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento um-para-um entre UserSystem e IdentityUser
        modelBuilder.Entity<UserSystem>()
            .HasOne(us => us.IdentityUser)
            .WithOne()
            .HasForeignKey<UserSystem>(us => us.Id)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento entre UserTransaction e IdentityUser
        modelBuilder.Entity<UserTransaction>()
            .HasOne(t => t.User)
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relacionamento entre UserBalance e IdentityUser
        modelBuilder.Entity<UserBalance>()
            .HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private void ConfigureUniqueIndexes(ModelBuilder modelBuilder)
    {
        // Índices únicos para IdentityUser
        modelBuilder.Entity<IdentityUser>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // Índices únicos para UserSystem
        modelBuilder.Entity<UserSystem>()
            .HasIndex(u => u.Nif)
            .IsUnique();
        modelBuilder.Entity<UserSystem>()
            .HasIndex(u => u.CreditCard)
            .IsUnique();

        // Índice único para UserId na tabela UserBalance
        modelBuilder.Entity<UserBalance>()
            .HasIndex(b => b.UserId)
            .IsUnique();
    }

    private void ConfigureEntities(ModelBuilder modelBuilder)
    {
        // Configuração da entidade ParkingUsage
        modelBuilder.Entity<ParkingUsage>(entity =>
        {
            entity.Property(e => e.EntryTime).HasColumnType("datetime");
            entity.Property(e => e.ExitTime).HasColumnType("datetime").IsRequired(false);
            entity.Property(e => e.TotalTimeMinutes).HasColumnName("total_time_minutes").HasDefaultValue(0);
            entity.Property(e => e.Price).HasColumnName("price").HasColumnType("decimal(10, 2)").HasDefaultValue(0.0m);
        });

        // Configuração da entidade UserTransaction
        modelBuilder.Entity<UserTransaction>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity.Property(t => t.UserId).IsRequired();
            entity.Property(t => t.Amount).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(t => t.TransactionDate).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(255).IsRequired();
            entity.Property(t => t.Type)
                .HasMaxLength(50)
                .IsRequired()
                .HasConversion<string>(); // Armazena o tipo como string ("Credito" ou "Debito")
        });

        // Configuração da entidade UserBalance
        modelBuilder.Entity<UserBalance>(entity =>
        {
            entity.HasKey(b => b.UserId);
            entity.Property(b => b.CurrentBalance).HasColumnType("decimal(18,2)").IsRequired();
        });
    }
}