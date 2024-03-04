using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class DiscussionTopic
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Discussion> Discussions { get; set; }

    }
}
