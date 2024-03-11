using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class AuctionBet
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public string UserId { get; set; }
        public int AuctionId { get; set; }

        public virtual User User { get; set; }
        public virtual Auction Auction { get; set; }
        
    }
}
