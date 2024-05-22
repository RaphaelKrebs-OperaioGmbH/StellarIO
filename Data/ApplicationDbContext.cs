using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StellarIO.Models;

public class ApplicationDbContext : IdentityDbContext<User>
{
    public DbSet<Building> Buildings { get; set; }
    public DbSet<Fleet> Fleets { get; set; }
    public DbSet<Galaxy> Galaxies { get; set; }
    public DbSet<GalaxySystem> GalaxySystems { get; set; }
    public DbSet<Planet> Planets { get; set; }
    public DbSet<Science> Sciences { get; set; }
    public DbSet<Ship> Ships { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User - Planet relationship
        modelBuilder.Entity<Planet>()
            .HasOne(p => p.User)
            .WithMany(u => u.Planets)
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Fleet relationships
        modelBuilder.Entity<Fleet>()
            .HasOne(f => f.OriginPlanet)
            .WithMany()
            .HasForeignKey(f => f.OriginPlanetId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fleet>()
            .HasOne(f => f.DestinationPlanet)
            .WithMany()
            .HasForeignKey(f => f.DestinationPlanetId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Fleet>()
            .HasOne(f => f.User)
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Planet - Building relationship
        modelBuilder.Entity<Building>()
            .HasOne(b => b.Planet)
            .WithMany(p => p.Buildings)
            .HasForeignKey(b => b.PlanetId)
            .OnDelete(DeleteBehavior.Cascade);

        // Science - User relationship
        modelBuilder.Entity<Science>()
            .HasOne(s => s.User)
            .WithMany(u => u.Sciences)
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Galaxy - GalaxySystem relationship
        modelBuilder.Entity<GalaxySystem>()
            .HasOne(gs => gs.Galaxy)
            .WithMany(g => g.Systems)
            .HasForeignKey(gs => gs.GalaxyId)
            .OnDelete(DeleteBehavior.Cascade);

        // GalaxySystem - Planet relationship
        modelBuilder.Entity<Planet>()
            .HasOne(p => p.System)
            .WithMany(gs => gs.Planets)
            .HasForeignKey(p => p.SystemId)
            .OnDelete(DeleteBehavior.Cascade);

        // Ship - Science relationship (for required science)
        modelBuilder.Entity<Ship>()
            .HasOne(s => s.ScienceRequired)
            .WithMany()
            .HasForeignKey(s => s.ScienceRequiredId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
