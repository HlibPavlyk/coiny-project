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
        }
    }
}
