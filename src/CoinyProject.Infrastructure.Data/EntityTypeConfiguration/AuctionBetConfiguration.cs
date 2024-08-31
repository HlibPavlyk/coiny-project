using CoinyProject.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.EntityTypeConfiguration
{
    internal class AuctionBetConfiguration : IEntityTypeConfiguration<AuctionBet>
    {
        public void Configure(EntityTypeBuilder<AuctionBet> builder)
        {
            builder.HasKey(al => al.Id);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");

            builder.HasOne(auctionBet => auctionBet.User)
               .WithMany(user => user.AuctionBets)
               .HasForeignKey(auctionBet => auctionBet.UserId)
               .OnDelete(DeleteBehavior.NoAction);




        }
    }
}
