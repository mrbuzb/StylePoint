using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
{
    public void Configure(EntityTypeBuilder<CartItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.User)
               .WithMany(u => u.CartItems)
               .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.ProductVariant)
               .WithMany()
               .HasForeignKey(x => x.ProductVariantId);
    }
}