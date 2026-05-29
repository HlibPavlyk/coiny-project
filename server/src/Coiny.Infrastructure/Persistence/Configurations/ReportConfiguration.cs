using Coiny.Domain.Entities;
using Coiny.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Reason).HasConversion<string>().HasMaxLength(32);
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(32);

        builder.Property(r => r.ReporterIp).HasMaxLength(64);
        builder.Property(r => r.Note).HasMaxLength(500);
        builder.Property(r => r.ResolutionNote).HasMaxLength(500);

        builder.Property(r => r.CreatedAt).HasColumnType("timestamptz");
        builder.Property(r => r.ResolvedAt).HasColumnType("timestamptz");

        // Admin queue: only un-resolved reports, sorted by age
        builder.HasIndex(r => new { r.Status, r.CreatedAt })
            .HasFilter($"\"{nameof(Report.Status)}\" = '{nameof(ReportStatus.Open)}'");

        builder.HasOne(r => r.Lot)
            .WithMany()
            .HasForeignKey(r => r.LotId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Reporter)
            .WithMany()
            .HasForeignKey(r => r.ReporterUserId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.Resolver)
            .WithMany()
            .HasForeignKey(r => r.ResolvedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
