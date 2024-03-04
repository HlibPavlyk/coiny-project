using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Core.Domain.Entities
{
    public class User : IdentityUser
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public int RoleId { get; set; }

        public virtual UserRole Role { get; set; }
        public virtual ICollection<Album> Albums { get; set; }
        public virtual ICollection<AuctionBet> AuctionBets { get; set; }
        public virtual ICollection<FavoriteAlbums> FavoriteAlbums { get; set; }
        public virtual ICollection<Discussion> Discussions { get; set; }
        public virtual ICollection<DiscussionMessage> DiscussionMessages { get; set; }
    }
}
