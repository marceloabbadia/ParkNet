
using ProjParkNet.Data.Entities;

namespace ProjParkNet.Data;

//public class ParkingDbContext : IdentityDbContext
public class ParkingDbContext : IdentityDbContext<IdentityUser>

{
    public ParkingDbContext(DbContextOptions<ParkingDbContext> options)
    //public class ParkingDbContext : IdentityDbContext
        : base(options)
    {
    }

    public DbSet<Parking> Parkings { get; set; }
    public DbSet<ParkingFloor> Floors { get; set; }
    public DbSet<ParkingSpot> ParkingSpots { get; set; }
    public DbSet<ParkingUsage> ParkingUsages { get; set; }
    public DbSet<UserSystem> UserSystems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configurações dos relacionamentos
        modelBuilder.Entity<ParkingFloor>()
            .HasOne(pf => pf.Parking)
            .WithMany(p => p.Floors)
            .HasForeignKey(pf => pf.ParkingId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParkingSpot>()
            .HasOne(ps => ps.ParkingFloor)
            .WithMany(pf => pf.Spots)
            .HasForeignKey(ps => ps.ParkingFloorId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParkingUsage>()
            .HasOne(pu => pu.ParkingSpot)
            .WithMany()
            .HasForeignKey(pu => pu.ParkingSpotId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<ParkingUsage>()
            .HasOne(pu => pu.User)
            .WithMany()
            .HasForeignKey(pu => pu.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<ParkingUsage>()
            .HasOne(pu => pu.ParkingSpot)
            .WithMany()
            .HasForeignKey(pu => pu.ParkingSpotId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Parking>()
            .Property(p => p.PricePerHour)
            .HasPrecision(10, 2);

        modelBuilder.Entity<Parking>()
            .Property(p => p.PricePerMinute)
            .HasPrecision(10, 2);

        modelBuilder.Entity<ParkingUsage>()
           .Property(pu => pu.Price)
           .HasPrecision(10, 2);

        modelBuilder.Entity<ParkingUsage>()
            .HasIndex(pu => pu.UserId);

        modelBuilder.Entity<ParkingUsage>()
            .HasIndex(pu => pu.ParkingSpotId);

        modelBuilder.Entity<UserSystem>()
           .HasOne(us => us.IdentityUser) // UserSystem tem um IdentityUser
           .WithOne() // IdentityUser tem um UserSystem
           .HasForeignKey<UserSystem>(us => us.Id);
    }
}



