using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Domain.Entities
{
    public class DiscussionMessage
    {
        public int Id { get; set; }
        public string Message { get; set; }
        public int DiscussionId { get; set; }
        public string? UserId { get; set; }

        public User? User { get; set; }
        public Discussion Discussion { get; set; }
    }
}
