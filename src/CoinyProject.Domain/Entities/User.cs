using Microsoft.AspNetCore.Identity;

namespace CoinyProject.Domain.Entities
{
    public class User : IdentityUser
    {
        public User(string userName, string email) : base(userName)
        {
            Email = email;
        }

        public int DiscussionRate { get; init; }

        public ICollection<Album>? Albums { get; init; }
        public ICollection<AuctionBet>? AuctionBets { get; init; }
        public ICollection<FavoriteAlbumElements>? FavoriteAlbumElements { get; init; }
        public ICollection<Discussion>? Discussions { get; init; }
        public ICollection<DiscussionMessage>? DiscussionMessages { get; init; }
    }
}
