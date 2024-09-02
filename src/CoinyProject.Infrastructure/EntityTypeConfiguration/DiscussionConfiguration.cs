using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class DiscussionConfiguration : IEntityTypeConfiguration<Discussion>
    {
        public void Configure(EntityTypeBuilder<Discussion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(x => x.Topic)
                .IsRequired();
            
            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(DiscussionStatus.Active);

            builder.HasMany(u => u.Messages)
               .WithOne(u => u.Discussion)
               .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

