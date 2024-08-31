﻿using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories.Interfaces
{
    public interface IDiscussionRepository : IBaseRepository<Discussion>
    {
        Task<IEnumerable<Discussion>> GetAllDiscussionsWithUserAndTopic();
        Task<Discussion> GetDiscussionWithUserAndTopicAndMessagesById(int? discussionId);
    }
}
