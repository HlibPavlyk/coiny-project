using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class ShipmentEventConfiguration : IEntityTypeConfiguration<ShipmentEvent>
{
    public void Configure(EntityTypeBuilder<ShipmentEvent> builder)
    {
        builder.HasKey(e => e.Id);

        // bigint identity — high-volume audit log.
        builder.Property(e => e.Id)
            .UseIdentityByDefaultColumn();

        builder.Property(e => e.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        builder.Property(e => e.ObservedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(e => new { e.ShipmentId, e.ObservedAt });

        // Idempotency for NP poll job — same shipment + status code + observed time → noop on insert.
        builder.HasIndex(e => new { e.ShipmentId, e.NpStatusCode, e.ObservedAt })
            .IsUnique();

        // FK relationship is owned by ShipmentConfiguration.HasMany(s => s.Events).
    }
}
