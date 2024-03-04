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
