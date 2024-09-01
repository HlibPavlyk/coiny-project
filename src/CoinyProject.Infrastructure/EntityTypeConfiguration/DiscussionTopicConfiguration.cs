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
    internal class DiscussionTopicConfiguration : IEntityTypeConfiguration<DiscussionTopic>
    {
        public void Configure(EntityTypeBuilder<DiscussionTopic> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(80);

            builder.HasMany(u => u.Discussions)
               .WithOne(u => u.DiscussionTopic)
               .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
