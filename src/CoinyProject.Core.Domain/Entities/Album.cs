using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class Album
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Rate { get; set; }
        public string? UserId { get; set; }

        public virtual User? User { get; set; }
        public virtual ICollection<AlbumElement>? Elements { get; set; }
        public virtual ICollection<FavoriteAlbums>? FavoriteAlbums { get; set; }
    }
}
