using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public int AlbumElementId { get; set; }
        public float StartPrice { get; set; }
        public float BetDelta { get; set; } 
        public DateTime StartTime { get; set; }
        public DateTime ExpirationTime { get; set; }
        public bool IsSoldEarlier { get; set; }

        public virtual AlbumElement AlbumElement { get; set; }
        public virtual ICollection<AuctionBet> AuctionBets { get; set; }
    }
}
