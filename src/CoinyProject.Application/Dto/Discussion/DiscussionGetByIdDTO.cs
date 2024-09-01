using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public record DiscussionGetByIdDTO(
        int Id,
        string Name,
        string Username,
        string Topic,
        IEnumerable<DiscussionMessageGetForViewDTO> Messages
        )
    { }
}
