using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public class DiscussionMessageCreateDTO
    {
        public string UserId { get; set; }
        public string Message { get; set; }
        public int DiscussionId { get; set; }
    }
}
