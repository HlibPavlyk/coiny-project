using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Album
{
    public record AlbumElementGetDTO(int Id, string Name, string? Description, string ImageURL)
    {
        
    }
}
