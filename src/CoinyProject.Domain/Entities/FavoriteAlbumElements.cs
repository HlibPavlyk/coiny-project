using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Domain.Entities
{
    public class FavoriteAlbumElements
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public Guid AlbumElementId { get; set; }

        public User User { get; set; }
        public AlbumElement AlbumElement { get; set; } 
    }
}
