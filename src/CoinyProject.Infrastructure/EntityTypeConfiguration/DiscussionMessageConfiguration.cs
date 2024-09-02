using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class DiscussionMessageConfiguration : IEntityTypeConfiguration<DiscussionMessage>
    {
        public void Configure(EntityTypeBuilder<DiscussionMessage> builder)
        {
            builder.HasKey(x => x.Id);
            
            builder.Property(x => x.Message)
                .IsRequired()
                .HasMaxLength(300);

            builder.HasOne(x => x.User)
                .WithMany(x => x.DiscussionMessages)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.Discussion)
                .WithMany(x => x.Messages)
                .HasForeignKey(x => x.DiscussionId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
