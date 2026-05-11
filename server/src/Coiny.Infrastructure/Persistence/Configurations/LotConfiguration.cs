using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class LotConfiguration : IEntityTypeConfiguration<Lot>
{
    public void Configure(EntityTypeBuilder<Lot> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.Description)
            .IsRequired();

        builder.Property(l => l.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(l => l.AuctionCloseJobId)
            .HasMaxLength(64);

        builder.Property(l => l.Condition)
            .HasConversion<string>()
            .HasMaxLength(20);

        // JSONB filtering disabled — search via Meilisearch
        builder.Property(l => l.Attributes)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(l => l.StartsAt)
            .HasColumnType("timestamptz");

        builder.Property(l => l.EndsAt)
            .HasColumnType("timestamptz");

        builder.Property(l => l.CreatedAt)
            .HasColumnType("timestamptz");

        builder.Property(l => l.UpdatedAt)
            .HasColumnType("timestamptz");

        builder.Property(l => l.DeletedAt)
            .HasColumnType("timestamptz");

        builder.HasQueryFilter(l => !l.IsDeleted);

        builder.HasIndex(l => new { l.Status, l.EndsAt })
            .HasFilter("\"Status\" = 'Active'");

        builder.HasIndex(l => new { l.CategoryId, l.Status, l.CreatedAt });

        builder.HasIndex(l => new { l.SellerId, l.Status });

        builder.HasOne(l => l.Seller)
            .WithMany()
            .HasForeignKey(l => l.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Category)
            .WithMany(c => c.Lots)
            .HasForeignKey(l => l.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Bid>()
            .WithMany()
            .HasForeignKey(l => l.WinningBidId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
