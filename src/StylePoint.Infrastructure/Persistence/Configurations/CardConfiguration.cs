using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(x => x.CardId);
        builder.Property(u => u.CardNumber).IsRequired(true).HasMaxLength(16);

        builder.HasMany(x => x.Payments)
            .WithOne(x => x.Card)
            .HasForeignKey(x=>x.CardId);

        
    }
}
