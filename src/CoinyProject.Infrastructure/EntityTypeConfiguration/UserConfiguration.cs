using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.Email)
                .HasMaxLength(50)
                .IsRequired();
            
            builder.HasIndex(u => u.Email)
                .IsUnique();

            builder.Property(u => u.UserName)
                .HasMaxLength(50)
                .IsRequired();

            builder.Property(u => u.DiscussionRate)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasMany(u => u.Albums)
                .WithOne(u => u.User).OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.Discussions)
                .WithOne(u => u.User).OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(u => u.AuctionBets)
                .WithOne(u => u.User).OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(u => u.FavoriteAlbumElements)
                .WithOne(u => u.User)
                .HasForeignKey(u => u.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
