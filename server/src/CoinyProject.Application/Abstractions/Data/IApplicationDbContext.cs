using CoinyProject.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Application.Abstractions.Data;

public interface IApplicationDbContext
{
    DbSet<Album> Albums { get; }
    DbSet<AlbumElement> AlbumElements { get; }
    DbSet<Auction> Auctions { get; }
    DbSet<AuctionBet> AuctionBets { get; }
    DbSet<Discussion> Discussions { get; }
    DbSet<DiscussionMessage> DiscussionMessages { get; }
    DbSet<FavoriteAlbumElements> FavoriteAlbumElements { get; }
    DbSet<User> Users { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
