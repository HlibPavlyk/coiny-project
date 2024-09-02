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
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(x => x.Description)
                .HasMaxLength(200)
                .IsRequired(false);
            
            builder.Property(x => x.Rate)
                .IsRequired()
                .HasDefaultValue(0);

            builder.Property(x => x.ImageURL)
                .IsRequired();
            
            builder.HasOne(x => x.Album)
                .WithMany(x => x.Elements)
                .HasForeignKey(x => x.AlbumId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
