using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class AuctionBet
    {
        public int Id { get; set; }
        public float Price { get; set; }
        public User User { get; set; }
    }
}
