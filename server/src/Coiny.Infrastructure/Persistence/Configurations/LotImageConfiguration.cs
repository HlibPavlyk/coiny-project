using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class LotImageConfiguration : IEntityTypeConfiguration<LotImage>
{
    public void Configure(EntityTypeBuilder<LotImage> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.StorageKey)
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(i => i.PublicUrl)
            .HasMaxLength(1024)
            .IsRequired();

        builder.Property(i => i.CreatedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(i => new { i.LotId, i.DisplayOrder });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_LotImages_DisplayOrder",
            "\"DisplayOrder\" >= 0 AND \"DisplayOrder\" < 5"));

        builder.HasOne(i => i.Lot)
            .WithMany(l => l.Images)
            .HasForeignKey(i => i.LotId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
