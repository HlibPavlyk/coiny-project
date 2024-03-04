using CoinyProject.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.EntityTypeConfiguration
{
    internal class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.Property(u => u.FirstName)
                .IsRequired(true)
                .HasMaxLength(50);

            builder.Property(u => u.LastName)
                .IsRequired(false)
                .HasMaxLength(50);

            builder.Property(u => u.Email)
                .IsRequired(true);

            builder.Property(u => u.UserName)
                .HasMaxLength(30)
                .IsRequired(true);

            builder.Property(u => u.PasswordHash)
                .IsRequired(true);

            builder.HasMany(u => u.Albums)
                .WithOne(u => u.User).
                OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(u => u.DiscussionMessages)
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
