using Xunit;
using CoinyProject.Application.AlbumServices.Services;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Core.Domain.Entities;
using CoinyProject.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using FluentAssertions;
using CoinyProject.Application.AutoMapper;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Net.Sockets;
using CoinyProject.Infrastructure.Data.Interfaces;
using Microsoft.AspNetCore.Routing;

namespace CoinyProject.Application.Tests.Servcices
{
    public class AlbumServiceUnitTest
    {
        private readonly Mock<IApplicationDBContext> _dbContext;
        private readonly IMapper _mapper;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironment;

        public AlbumServiceUnitTest()
        {
            _dbContext = new();
            _webHostEnvironment = new();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            _mapper = new Mapper(configuration);
        }

        [Fact]
        public async Task AddAlbum_ReturnsAlbumId_IfAddingAlbumIsCorrect()
        {
            var albumCreating = new AlbumCreating ("Test Album", "Test Description" );
            var userId = "user123";
            var albumId = 1;

            var albumList = new List<Album>();
            var dbSetMock = new Mock<DbSet<Album>>();
            
            _dbContext.Setup(Album => Album.Albums).Returns(dbSetMock.Object);
            dbSetMock
                .Setup(_ => _.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()))
                .Callback((Album model, CancellationToken token) => { model.Id = albumId; albumList.Add(model); })
                .Returns((Album model, CancellationToken token) => ValueTask.FromResult((EntityEntry<Album>)null));
            _dbContext.Setup(db => db.SaveChangesAsync())
                 .ReturnsAsync(1);

            var albumService = new AlbumService(_dbContext.Object, _webHostEnvironment.Object, _mapper);

            var result = await albumService.AddAlbum(albumCreating, userId);

            dbSetMock.Verify(m => m.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()), Times.Once);
            _dbContext.Verify(db => db.SaveChangesAsync(), Times.Once);
            result.Should().Be(albumId);
            albumList.Should().ContainSingle(
                a => a.Name == albumCreating.Name && 
                a.Description == albumCreating.Description 
                && a.UserId == userId);
        }

        [Fact]
        public async Task AddAlbum_ReturnsException_IfInputArgumentsAreNull()
        {
            AlbumCreating? albumCreating = null;
            string? userId = null;
            var albumList = new List<Album>();
            var dbSetMock = new Mock<DbSet<Album>>();
            _dbContext.Setup(Album => Album.Albums).Returns(dbSetMock.Object);
            var albumService = new AlbumService(_dbContext.Object, _webHostEnvironment.Object, _mapper);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.AddAlbum(albumCreating, userId));

            dbSetMock.Verify(m => m.AddAsync(It.IsAny<Album>(), It.IsAny<CancellationToken>()), Times.Never);
            _dbContext.Verify(db => db.SaveChangesAsync(), Times.Never);
        }
    }
}
