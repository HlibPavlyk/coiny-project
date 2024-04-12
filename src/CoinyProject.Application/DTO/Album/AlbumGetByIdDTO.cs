using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Album
{
    public record AlbumGetByIdDTO(int Id, string Name, string? Description, 
        int Rate, string UserId, ICollection<AlbumElementGetDTO> Elements)
    {
        
    }
}
