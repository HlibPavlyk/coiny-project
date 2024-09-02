using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class AuctionConfiguration : IEntityTypeConfiguration<Auction>
    {
        public void Configure(EntityTypeBuilder<Auction> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.StartPrice)
                .HasColumnType("decimal(10,2)");
            
            builder.Property(x => x.BetDelta)
                .HasColumnType("decimal(10,2)");

            builder.Property(x => x.StartTime)
                .HasDefaultValueSql("getdate()");
            
            builder.Property(x => x.Status)
                .IsRequired()
                .HasDefaultValue(AuctionStatus.Active);

            builder.HasOne(x => x.AlbumElement)
                .WithOne(x => x.Auction)
                .HasForeignKey<Auction>(x => x.AlbumElementId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
