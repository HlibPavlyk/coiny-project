using Microsoft.AspNetCore.Identity;

namespace CoinyProject.Domain.Entities
{
    public sealed class User : IdentityUser
    {
        public User(string userName, string email) : base(userName)
        {
            Email = email;
        }

        public ICollection<Album>? Albums { get; set; }
        public ICollection<AuctionBet>? AuctionBets { get; set; }
        public ICollection<FavoriteAlbums>? FavoriteAlbums { get; set; }
        public ICollection<Discussion>? Discussions { get; set; }
    }
}
