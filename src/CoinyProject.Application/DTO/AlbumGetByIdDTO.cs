using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumGetByIdDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public int Rate { get; set; }
        public string UserId { get; set; }
        public ICollection<AlbumElementGetDTO> Elements { get; set; }
    }
}
