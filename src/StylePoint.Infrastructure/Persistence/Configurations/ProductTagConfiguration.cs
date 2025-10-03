using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StylePoint.Domain.Entities;

namespace StylePoint.Infrastructure.Persistence.Configurations;

public class ProductTagConfiguration : IEntityTypeConfiguration<ProductTag>
{
    public void Configure(EntityTypeBuilder<ProductTag> builder)
    {
        builder.HasKey(x => new { x.ProductId, x.TagId });

        builder.HasOne(x => x.Product)
               .WithMany(p => p.ProductTags)
               .HasForeignKey(x => x.ProductId);

        builder.HasOne(x => x.Tag)
               .WithMany(t => t.ProductTags)
               .HasForeignKey(x => x.TagId);
    }
}