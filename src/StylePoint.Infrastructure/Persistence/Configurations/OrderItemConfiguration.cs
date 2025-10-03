using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(x => x.OrderId);

        builder.HasOne(x => x.ProductVariant)
               .WithMany()
               .HasForeignKey(x => x.ProductVariantId);
    }
}