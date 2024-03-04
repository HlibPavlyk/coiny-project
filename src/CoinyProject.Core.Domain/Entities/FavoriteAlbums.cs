using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class FavoriteAlbums
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int AlbumId { get; set; }

        public virtual User User { get; set; }
        public virtual Album Album { get; set; } 
    }
}
