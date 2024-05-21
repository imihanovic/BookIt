using System.Reflection;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BookIt.Core.Common;
using BookIt.Core.Entities;
using BookIt.Shared.Services;
using BookIt.Core.Entities.Identity;

namespace BookIt.DataAccess.Persistence;

public class DatabaseContext : IdentityDbContext<ApplicationUser>
{
    private readonly IClaimService _claimService;

    public DatabaseContext(DbContextOptions options, IClaimService claimService) : base(options)
    {
        _claimService = claimService;
    }

    public DbSet<Table> Tables { get; set; }
    public DbSet<Reservation> Reservations { get; set; }
    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Dish> Dishes { get; set; }
    public DbSet<RestaurantDish> RestaurantDishes { get; set; }
    


    protected override void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(builder);
            
        builder.Entity<Table>()
        .HasMany(e => e.Reservations)
        .WithMany(e => e.Tables)
        .UsingEntity("TableReservation");

        builder.Entity<ApplicationUser>()
        .HasMany(e => e.CustomerReservations)
        .WithOne(e => e.Customer)
        .IsRequired(false);

        builder.Entity<ApplicationUser>()
        .HasOne(e => e.Restaurant)
        .WithOne(e => e.Manager)
        .HasForeignKey<Restaurant>(e => e.ManagerId)
        .IsRequired();

        builder.Entity<Restaurant>()
        .HasMany(e => e.Tables)
        .WithOne(e => e.Restaurant)
        .IsRequired();
    }

    public new async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<IAuditedEntity>())
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _claimService.GetUserId();
                    entry.Entity.CreatedOn = DateTime.Now;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedBy = _claimService.GetUserId();
                    entry.Entity.UpdatedOn = DateTime.Now;
                    break;
            }

        return await base.SaveChangesAsync(cancellationToken);
    }
}
