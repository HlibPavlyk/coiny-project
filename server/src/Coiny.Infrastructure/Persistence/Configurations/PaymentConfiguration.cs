using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.Property(p => p.RateUsedUahPerUsd)
            .HasColumnType("decimal(10, 4)");

        // Nullable: AuctionCloseJob pre-creates the Payment row with intent id null. The id is
        // populated when the buyer first opens the checkout (CreatePaymentIntentHandler mints
        // the Stripe intent at that point).
        builder.Property(p => p.StripePaymentIntentId)
            .HasMaxLength(64);

        builder.Property(p => p.LastWebhookEventId)
            .HasMaxLength(64);

        builder.Property(p => p.DueAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.AuthorizedAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.CapturedAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.CancelledAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.ReminderSentAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.CreatedAt)
            .HasColumnType("timestamptz");

        builder.Property(p => p.UpdatedAt)
            .HasColumnType("timestamptz");

        // One payment per lot.
        builder.HasIndex(p => p.LotId)
            .IsUnique();

        // Unique among non-null values. PostgreSQL treats NULLs as distinct in unique indexes, so
        // multiple pre-created rows (intent id null) coexist without violating uniqueness.
        builder.HasIndex(p => p.StripePaymentIntentId)
            .IsUnique();

        // Partial index for the non-payment cancellation job.
        builder.HasIndex(p => new { p.Status, p.DueAt })
            .HasFilter("\"Status\" = 'PendingAuthorization'");

        builder.HasOne<Lot>()
            .WithMany()
            .HasForeignKey(p => p.LotId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.BuyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(p => p.SellerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Shipment)
            .WithOne(s => s.Payment!)
            .HasForeignKey<Shipment>(s => s.PaymentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
