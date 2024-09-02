using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Domain.Enums;

namespace CoinyProject.Domain.Entities
{
    public class Auction
    {
        public Guid Id { get; set; }
        public Guid AlbumElementId { get; set; }
        public decimal StartPrice { get; set; }
        public decimal BetDelta { get; set; } 
        public DateTime StartTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public AuctionStatus Status { get; set; }

        public AlbumElement AlbumElement { get; set; }
        public ICollection<AuctionBet> AuctionBets { get; set; }
    }
}
