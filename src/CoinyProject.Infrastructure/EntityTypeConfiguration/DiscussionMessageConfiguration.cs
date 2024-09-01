using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class DiscussionMessageConfiguration : IEntityTypeConfiguration<DiscussionMessage>
    {
        public void Configure(EntityTypeBuilder<DiscussionMessage> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(x => x.UserId)
                .IsRequired(false);
        }
    }
}
