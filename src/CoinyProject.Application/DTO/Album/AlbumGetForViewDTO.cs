using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumGetForViewDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Rate { get; set; }
        public string? TitleImageURL { get; set; }
        public bool IsFavorite { get; set; }

    }
}
