using Microsoft.AspNetCore.Identity;

namespace CoinyProject.Domain.Entities
{
    public class User : IdentityUser
    {
        public User(string userName, string email) : base(userName)
        {
            Email = email;
        }

        public int DiscussionRate { get; set; }

        public ICollection<Album>? Albums { get; set; }
        public ICollection<AuctionBet>? AuctionBets { get; set; }
        public ICollection<FavoriteAlbumElements>? FavoriteAlbumElements { get; set; }
        public ICollection<Discussion>? Discussions { get; set; }
    }
}
