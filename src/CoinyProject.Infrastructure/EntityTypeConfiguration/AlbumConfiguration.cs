﻿using CoinyProject.Domain.Entities;
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
                .IsRequired(true)
                .HasMaxLength(30);

            builder.Property(al => al.Description)
                .IsRequired(false);

            builder.Property(al => al.Rate).HasDefaultValue(0);

            builder.HasMany(u => u.Elements)
                .WithOne(u => u.Album).
                OnDelete(DeleteBehavior.Cascade);
        }
    }
}