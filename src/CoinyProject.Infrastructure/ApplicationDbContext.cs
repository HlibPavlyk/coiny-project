using CoinyProject.Domain.Abstractions;
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
        
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var currentDateTime = DateTime.Now;
            var entries = ChangeTracker.Entries().ToList();

            var updatedEntries = entries.Where(e => e is { Entity: IUpdateable, State: EntityState.Modified }).ToList();

            updatedEntries.ForEach(e =>
            {
                ((IUpdateable)e.Entity).UpdatedAt = currentDateTime;

                if (e.Entity is AlbumElement albumItem)
                {
                    var album = albumItem.Album;
                    album.UpdatedAt = currentDateTime;
                    Entry(album).State = EntityState.Modified;
                }
            });

            return await base.SaveChangesAsync(cancellationToken);
        }


    }
}
