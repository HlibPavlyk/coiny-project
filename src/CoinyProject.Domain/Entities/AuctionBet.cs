using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class AuctionBet
    {
        public Guid Id { get; set; }
        public decimal Price { get; set; }
        public AuctionBetStatus Status { get; set; }
        public Guid UserId { get; set; }
        public Guid AuctionId { get; set; }

        public User User { get; set; }
        public Auction Auction { get; set; }
        
    }
}
