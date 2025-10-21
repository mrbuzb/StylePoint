using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Name).IsRequired().HasMaxLength(150);
        builder.Property(x => x.Description).HasMaxLength(500);
        builder.Property(x => x.Price).HasColumnType("decimal(18,2)");
        builder.Property(x => x.DiscountPrice).HasColumnType("decimal(18,2)");
        builder.Property(x => x.SecretCode).IsRequired();
        builder.HasIndex(x=>x.SecretCode).IsUnique();

        builder.HasOne(x => x.Category)
               .WithMany(c => c.Products)
               .HasForeignKey(x => x.CategoryId);

        builder.HasOne(x => x.Brand)
               .WithMany(b => b.Products)
               .HasForeignKey(x => x.BrandId);
    }
}