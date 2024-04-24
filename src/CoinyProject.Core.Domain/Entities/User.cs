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
        public User(string userName, string firstName, string lastName) : base(userName)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = userName;
        }

        public ICollection<Album>? Albums { get; set; }
        public ICollection<AuctionBet> AuctionBets { get; set; }
        public ICollection<FavoriteAlbums>? FavoriteAlbums { get; set; }
        public ICollection<Discussion>? Discussions { get; set; }
    }
}
