using Coiny.Domain.Entities;
using Coiny.Infrastructure.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Coiny.Infrastructure.Persistence.Configurations;

public class CategoryConfiguration : IEntityTypeConfiguration<Category>
{
    public void Configure(EntityTypeBuilder<Category> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .UseIdentityColumn();

        builder.Property(c => c.Slug)
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(c => c.Slug)
            .IsUnique();

        builder.Property(c => c.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.SubcategoryKind)
            .HasConversion<string>()
            .HasMaxLength(30);

        builder.HasIndex(c => new { c.ParentId, c.DisplaySort });

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_Categories_IsLeaf_SubcategoryKind",
            "(\"IsLeaf\" = false AND \"SubcategoryKind\" IS NULL) OR (\"IsLeaf\" = true AND \"SubcategoryKind\" IS NOT NULL)"));

        builder.HasOne(c => c.Parent)
            .WithMany(c => c.Children)
            .HasForeignKey(c => c.ParentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(CategorySeed.All);
    }
}
