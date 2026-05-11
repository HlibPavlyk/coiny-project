using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class BidConfiguration : IEntityTypeConfiguration<Bid>
{
    public void Configure(EntityTypeBuilder<Bid> builder)
    {
        builder.HasKey(b => b.Id);

        builder.Property(b => b.CreatedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(b => new { b.LotId, b.AmountUahKopiykas })
            .IsDescending(false, true);

        builder.HasIndex(b => new { b.BidderId, b.CreatedAt })
            .IsDescending(false, true);

        builder.HasOne(b => b.Lot)
            .WithMany()
            .HasForeignKey(b => b.LotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Bidder)
            .WithMany()
            .HasForeignKey(b => b.BidderId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
