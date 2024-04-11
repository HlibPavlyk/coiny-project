using CoinyProject.Core.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Interfaces
{
    public interface IApplicationDBContext
    {
        DbSet<Album> Albums { get; }
        DbSet<AlbumElement> AlbumElements { get; }
        DbSet<Auction> Auctions { get; }
        DbSet<AuctionBet> AuctionBets { get; }
        DbSet<Discussion> Discussions { get; }
        DbSet<DiscussionMessage> DiscussionMessages { get; }
        DbSet<DiscussionTopic> DiscussionTopics { get; }
        DbSet<FavoriteAlbums> FavoriteAlbums { get; }
        DbSet<User> Users { get; set; }
        Task<int> SaveChangesAsync();
        int SaveChanges();
        
    }
}
