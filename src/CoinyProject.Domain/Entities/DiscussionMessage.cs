﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Domain.Entities
{
    public class DiscussionMessage
    {
        public Guid Id { get; set; }
        public string Message { get; set; }
        public Guid DiscussionId { get; set; }
        public Guid UserId { get; set; }

        public User? User { get; set; }
        public Discussion Discussion { get; set; }
    }
}
