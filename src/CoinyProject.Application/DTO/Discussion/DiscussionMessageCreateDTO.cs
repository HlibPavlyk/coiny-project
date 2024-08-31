using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public record DiscussionMessageCreateDTO (
        string UserId, 
        string Message, 
        int DiscussionId
        )
    {}
}
