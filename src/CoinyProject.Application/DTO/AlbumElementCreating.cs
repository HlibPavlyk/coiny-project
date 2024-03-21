using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO
{
    public class AlbumElementCreating
    {
        public string Name { get; set; }
        public string? Description { get; set; }
        public int AlbumId { get; set; }
        public IFormFile Image { get; set; }
    }
}
