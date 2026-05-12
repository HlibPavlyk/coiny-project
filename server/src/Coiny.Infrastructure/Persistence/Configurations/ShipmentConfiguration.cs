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

        builder.Property(s => s.NovaPoshtaTtn)
            .HasMaxLength(32)
            .IsRequired();

        builder.Property(s => s.IntDocNumber)
            .HasMaxLength(32)
            .IsRequired();

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

        builder.HasIndex(s => s.PaymentId)
            .IsUnique();

        builder.HasIndex(s => s.NovaPoshtaTtn)
            .IsUnique();

        // Partial index for the NP polling job — only in-flight shipments.
        builder.HasIndex(s => s.Status)
            .HasFilter("\"Status\" NOT IN ('Delivered','Refused','Returned','Lost')");

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
