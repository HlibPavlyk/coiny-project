﻿using CoinyProject.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CoinyProject.Infrastructure
{
    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<Guid>, Guid>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumElement> AlbumElements { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<AuctionBet> AuctionBets { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
        public DbSet<FavoriteAlbumElements> FavoriteAlbumElements { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }
}