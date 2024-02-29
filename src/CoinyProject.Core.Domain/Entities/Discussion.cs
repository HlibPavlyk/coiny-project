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

        public DiscussionMessage RootQuestion { get; set; }
        public DiscussionTopic DiscussionTopic { get; set; }
        public ICollection<DiscussionMessage> Messages { get; set; }
    }
}
