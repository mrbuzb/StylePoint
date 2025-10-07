using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace AutoLedger.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {

        builder.ToTable("Users");

        builder.HasKey(u => u.UserId);

        builder.Property(u => u.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.LastName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.Password).IsRequired();
        builder.Property(u => u.Salt).IsRequired();
        builder.Property(u => u.ConfirmerId).IsRequired(false);

        builder.HasOne(u => u.Confirmer)
            .WithOne(c => c.User)
            .HasForeignKey<User>(u => u.ConfirmerId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Role)
               .WithMany(r => r.Users)
               .HasForeignKey(u => u.RoleId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Orders)
               .WithOne(o => o.User)
               .HasForeignKey(o => o.UserId);

        builder.HasOne(x => x.Card)
               .WithOne(o => o.User)
               .HasForeignKey<User>(o => o.UserId);

        builder.HasMany(x => x.CartItems)
               .WithOne(c => c.User)
               .HasForeignKey(c => c.UserId);

        builder.HasMany(x => x.Addresses)
               .WithOne(a => a.User)
               .HasForeignKey(a => a.UserId);

    }
}