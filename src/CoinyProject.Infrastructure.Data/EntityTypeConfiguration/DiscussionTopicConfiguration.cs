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
    internal class DiscussionTopicConfiguration : IEntityTypeConfiguration<DiscussionTopic>
    {
        public void Configure(EntityTypeBuilder<DiscussionTopic> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .HasMaxLength(50);
        }
    }
}
