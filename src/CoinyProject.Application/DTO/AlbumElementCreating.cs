using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumElementCreating
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public byte[]? Image { get; set; }
    }
}
