using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class DiscussionMessage
    {
        public int Id { get; set; }
        public User User { get; set; }
        public string Message { get; set; }
    }
}
