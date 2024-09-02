using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class AlbumConfiguration : IEntityTypeConfiguration<Album>
    {
        public void Configure(EntityTypeBuilder<Album> builder)
        {
            builder.HasKey(al => al.Id);

            builder.Property(al => al.Name)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(al => al.Description)
                .IsRequired(false)
                .HasMaxLength(200);

            builder.Property(al => al.Status)
                .IsRequired()
                .HasDefaultValue(AlbumStatus.NotApproved);

            builder.Property(al => al.Rate)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasOne(al => al.User)
                .WithMany(u => u.Albums)
                .HasForeignKey(al => al.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
