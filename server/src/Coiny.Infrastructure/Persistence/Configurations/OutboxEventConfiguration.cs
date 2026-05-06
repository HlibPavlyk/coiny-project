using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class OutboxEventConfiguration : IEntityTypeConfiguration<OutboxEvent>
{
    public void Configure(EntityTypeBuilder<OutboxEvent> builder)
    {
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .UseIdentityColumn();

        builder.Property(e => e.AggregateType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.EventType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Payload)
            .HasColumnType("jsonb")
            .HasDefaultValue("{}");

        builder.Property(e => e.CreatedAt)
            .HasColumnType("timestamptz");

        builder.Property(e => e.ProcessedAt)
            .HasColumnType("timestamptz");

        builder.HasIndex(e => e.Id)
            .HasFilter("\"ProcessedAt\" IS NULL");
    }
}
