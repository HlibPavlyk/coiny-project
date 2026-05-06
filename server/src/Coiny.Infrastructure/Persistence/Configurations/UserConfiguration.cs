using Coiny.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Email)
            .HasColumnType("citext");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.DisplayName)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(u => u.FullName)
            .HasMaxLength(200);

        builder.Property(u => u.BanReason)
            .HasMaxLength(500);

        builder.Property(u => u.StripeAccountId)
            .HasMaxLength(64);

        builder.Property(u => u.GoogleSubject)
            .HasMaxLength(255);

        builder.HasIndex(u => u.GoogleSubject)
            .IsUnique()
            .HasFilter("\"GoogleSubject\" IS NOT NULL");

        builder.HasIndex(u => u.StripeAccountId)
            .HasFilter("\"StripeAccountId\" IS NOT NULL");

        builder.Property(u => u.CreatedAt)
            .HasColumnType("timestamptz");

        builder.Property(u => u.UpdatedAt)
            .HasColumnType("timestamptz");

        builder.Property(u => u.BannedAt)
            .HasColumnType("timestamptz");
    }
}
