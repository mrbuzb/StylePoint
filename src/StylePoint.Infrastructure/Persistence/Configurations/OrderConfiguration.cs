using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TotalPrice).HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.User)
               .WithMany(u => u.Orders)
               .HasForeignKey(x => x.UserId);

        builder.HasOne(x => x.Address)
               .WithMany()
               .HasForeignKey(x => x.AddressId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}