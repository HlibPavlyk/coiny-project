using CoinyProject.Domain.Entities;
using CoinyProject.Infrastructure.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<User, ApplicationRole, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Album> Albums { get; init; }
        public DbSet<AlbumElement> AlbumElements { get; init; }
        public DbSet<Auction> Auctions { get; init; }
        public DbSet<AuctionBet> AuctionBets { get; init; }
        public DbSet<Discussion> Discussions { get; init; }
        public DbSet<DiscussionMessage> DiscussionMessages { get; init; }
        public DbSet<FavoriteAlbumElements> FavoriteAlbumElements { get; init; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ApplicationRole>().HasData(
                new ApplicationRole { Id = Guid.NewGuid(), Name = "User", NormalizedName = "USER" },
                new ApplicationRole { Id = Guid.NewGuid(), Name = "Moderator", NormalizedName = "MODERATOR" },
                new ApplicationRole { Id = Guid.NewGuid(), Name = "Administrator", NormalizedName = "ADMINISTRATOR" }
            );
        }

    }
}
