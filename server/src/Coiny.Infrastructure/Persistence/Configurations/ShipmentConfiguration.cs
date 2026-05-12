using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class ShipmentConfiguration : IEntityTypeConfiguration<Shipment>
{
    public void Configure(EntityTypeBuilder<Shipment> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        // Both TTN columns are nullable until the CreateTtnJob (sprint 3 task 10) populates them.
        // Shipment rows enter the table in Status = PendingTtn with these fields null.
        builder.Property(s => s.NovaPoshtaTtn)
            .HasMaxLength(32);

        builder.Property(s => s.IntDocNumber)
            .HasMaxLength(32);

        builder.Property(s => s.SenderCityRef)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.SenderWarehouseRef)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.RecipientCityRef)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.RecipientWarehouseRef)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(s => s.RecipientName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.RecipientPhone)
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.DeliveredAt)
            .HasColumnType("timestamptz");

        builder.Property(s => s.LastPolledAt)
            .HasColumnType("timestamptz");

        builder.Property(s => s.CreatedAt)
            .HasColumnType("timestamptz");

        builder.Property(s => s.UpdatedAt)
            .HasColumnType("timestamptz");

        // PaymentId is nullable while the shipment is in PendingTtn (no Payment row yet).
        // Partial unique index keeps the 1:1 invariant once the payment intent is created.
        builder.HasIndex(s => s.PaymentId)
            .IsUnique()
            .HasFilter("\"PaymentId\" IS NOT NULL");

        // Partial unique index — NULL TTNs are allowed during PendingTtn staging.
        builder.HasIndex(s => s.NovaPoshtaTtn)
            .IsUnique()
            .HasFilter("\"NovaPoshtaTtn\" IS NOT NULL");

        // Partial index for the NP polling job — only post-TTN, in-flight shipments.
        // PendingTtn rows are not yet known to NP, so they're excluded.
        builder.HasIndex(s => s.Status)
            .HasFilter("\"Status\" NOT IN ('PendingTtn','Delivered','Refused','Returned','Lost')");

        // FKs to User — kept Restrict so deleting a User never silently kills shipment records.
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(s => s.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Lot>()
            .WithMany()
            .HasForeignKey(s => s.LotId)
            .OnDelete(DeleteBehavior.Restrict);

        // Payment ↔ Shipment 1:1 is defined on the Payment side (PaymentConfiguration).

        builder.HasMany(s => s.Events)
            .WithOne(e => e.Shipment!)
            .HasForeignKey(e => e.ShipmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
