using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class Discussion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? UserId { get; set; }
        public int DiscussionTopicId { get; set; }

        public User? User { get; set; }
        public DiscussionTopic DiscussionTopic { get; set; }
        public ICollection<DiscussionMessage>? Messages { get; set; }
    }
}
