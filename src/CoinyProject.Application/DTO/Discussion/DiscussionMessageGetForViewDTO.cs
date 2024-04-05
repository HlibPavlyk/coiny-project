using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.DTO.Discussion
{
    public class DiscussionMessageGetForViewDTO
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Username { get; set; }
    }
}
