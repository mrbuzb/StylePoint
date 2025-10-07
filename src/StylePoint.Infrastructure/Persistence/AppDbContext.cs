using Microsoft.EntityFrameworkCore;
using StylePoint.Domain.Entities;

namespace AutoLedger.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<UserRole> UserRoles { get; set; }
    public DbSet<UserConfirme> Confirmers { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Category> Categories { get; set; }
    public DbSet<Brand> Brands { get; set; }
    public DbSet<Tag> Tags { get; set; } 
    public DbSet<Product> Products { get; set; }
    public DbSet<ProductVariant> ProductVariants { get; set; }
    public DbSet<ProductTag> ProductTags { get; set; } 
    public DbSet<CartItem> CartItems { get; set; } 
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<DeliveryAddress> DeliveryAddresses { get; set; }
    public DbSet<Discount> Discounts { get; set; } 
    public DbSet<Payment> Payments { get; set; } 
    public DbSet<Card> Cards { get; set; } 

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
