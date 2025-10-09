using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class UserDiscountConfiguration : IEntityTypeConfiguration<UserDiscount>
{
    public void Configure(EntityTypeBuilder<UserDiscount> builder)
    {
        builder.HasKey(ud => new { ud.UserId, ud.DiscountId });

        builder.HasOne(ud => ud.User)
               .WithMany(u => u.Discounts)
               .HasForeignKey(ud => ud.UserId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ud => ud.Discount)
               .WithMany(d => d.RedeemedUsers)
               .HasForeignKey(ud => ud.DiscountId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.Property(ud => ud.UsedAt)
               .IsRequired();
    }
}
