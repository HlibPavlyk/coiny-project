using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CoinyProject.Infrastructure.EntityTypeConfiguration
{
    internal class AuctionBetConfiguration : IEntityTypeConfiguration<AuctionBet>
    {
        public void Configure(EntityTypeBuilder<AuctionBet> builder)
        {
            builder.HasKey(al => al.Id);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");
            
            builder.Property(x => x.Status)
                .IsRequired()
                .HasConversion<string>()
                .HasDefaultValue(AuctionBetStatus.Winning);

            builder.HasOne(auctionBet => auctionBet.User)
               .WithMany(user => user.AuctionBets)
               .HasForeignKey(auctionBet => auctionBet.UserId)
               .OnDelete(DeleteBehavior.NoAction);
            
            builder.HasOne(x => x.Auction)
                .WithMany(x => x.AuctionBets)
                .HasForeignKey(x => x.AuctionId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasOne(x => x.User)
                .WithMany(x => x.AuctionBets)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.NoAction);

        }
    }
}
