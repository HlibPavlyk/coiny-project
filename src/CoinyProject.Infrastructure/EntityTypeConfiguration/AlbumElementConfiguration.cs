using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class AlbumElementConfiguration : IEntityTypeConfiguration<AlbumElement>
    {
        public void Configure(EntityTypeBuilder<AlbumElement> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(30)
                .IsRequired(true);

            builder.Property(x => x.Description)
                .IsRequired(false);

        }
    }
}
