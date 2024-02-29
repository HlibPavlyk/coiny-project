using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class Auction
    {
        public int Id { get; set; }
        public AlbumElement Lot { get; set; }
    }
}
