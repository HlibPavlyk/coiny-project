using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.Data.EntityTypeConfiguration
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Email)
                .HasMaxLength(50)
                .IsRequired(true);

            builder.Property(u => u.UserName)
                .HasMaxLength(50)
                .IsRequired(true);

            builder.Property(u => u.PasswordHash)
                .IsRequired(true);

            builder.HasMany(u => u.Albums)
                .WithOne(u => u.User).
                OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.Discussions)
                .WithOne(u => u.User).
                OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.AuctionBets)
                .WithOne(u => u.User).
                OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.FavoriteAlbums)
                .WithOne(u => u.User).
                OnDelete(DeleteBehavior.NoAction);
        }
    }
}
