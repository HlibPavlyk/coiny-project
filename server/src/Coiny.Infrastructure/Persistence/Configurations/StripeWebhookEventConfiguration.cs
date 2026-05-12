using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class StripeWebhookEventConfiguration : IEntityTypeConfiguration<StripeWebhookEvent>
{
    public void Configure(EntityTypeBuilder<StripeWebhookEvent> builder)
    {
        // PK is the Stripe event.id (evt_…), not a Guid.
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(80)
            .IsRequired();

        builder.Property(e => e.PayloadJson)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(e => e.ReceivedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.ProcessedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.ProcessingError)
            .HasMaxLength(1000);

        // Partial index for the RetryFailedWebhookJob — only unprocessed rows.
        builder.HasIndex(e => e.Id)
            .HasFilter("\"ProcessedAt\" IS NULL")
            .HasDatabaseName("IX_StripeWebhookEvent_Pending");
    }
}
