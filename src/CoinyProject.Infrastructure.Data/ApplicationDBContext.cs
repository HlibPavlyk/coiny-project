﻿using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data
{
    public class ApplicationDBContext : IdentityDbContext<User>, IApplicationDBContext
    {
       
        public ApplicationDBContext(DbContextOptions<ApplicationDBContext> options) : base(options) { }

        public DbSet<Album> Albums { get; set; }
        public DbSet<AlbumElement> AlbumElements { get; set; }
        public DbSet<Auction> Auctions { get; set; }
        public DbSet<AuctionBet> AuctionBets { get; set; }
        public DbSet<Discussion> Discussions { get; set; }
        public DbSet<DiscussionMessage> DiscussionMessages { get; set; }
        public DbSet<DiscussionTopic> DiscussionTopics { get; set; }
        public DbSet<FavoriteAlbums> FavoriteAlbums { get; set; }

        public async Task<int> SaveChangesAsync()
        {
            return await base.SaveChangesAsync();
        }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDBContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

    }
}
