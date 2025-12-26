using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Handlers;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CoinyProject.UnitTests.Services;

public class AlbumsHandlerUnitTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly AlbumsHandler _handler;

    public AlbumsHandlerUnitTests()
    {
        _mapperMock = new Mock<IMapper>();
        _contextMock = new Mock<IApplicationDbContext>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _handler = new AlbumsHandler(_contextMock.Object, _mapperMock.Object, _identityServiceMock.Object);
    }

    private static DefaultHttpContext CreateHttpContext(bool isAuthenticated, string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        var identity = new ClaimsIdentity(claims, isAuthenticated ? "auth" : "");
        var user = new ClaimsPrincipal(identity);

        return new DefaultHttpContext { User = user };
    }

    private static Mock<DbSet<T>> CreateMockDbSet<T>(List<T> data) where T : class
    {
        var queryable = data.AsQueryable();
        var mockSet = new Mock<DbSet<T>>();

        mockSet.As<IQueryable<T>>().Setup(m => m.Provider).Returns(queryable.Provider);
        mockSet.As<IQueryable<T>>().Setup(m => m.Expression).Returns(queryable.Expression);
        mockSet.As<IQueryable<T>>().Setup(m => m.ElementType).Returns(queryable.ElementType);
        mockSet.As<IQueryable<T>>().Setup(m => m.GetEnumerator()).Returns(queryable.GetEnumerator());

        return mockSet;
    }

    [Fact]
    public async Task CreateAlbum_ReturnsFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var request = new CreateAlbumRequest
        {
            Name = "test_name",
            Description = "test_description"
        };

        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(CreateHttpContext(false, string.Empty));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task UpdateAlbum_ReturnsFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var request = new UpdateAlbumRequest(Guid.NewGuid(),
            new UpdateAlbumModel
            {
                Name = "test_name", Description = "test_description"
            });
        
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(CreateHttpContext(false, string.Empty));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task DeactivateAlbum_ReturnsFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var request = new DeactivateAlbumRequest(Guid.NewGuid());
        _httpContextAccessorMock.Setup(x => x.HttpContext).Returns(CreateHttpContext(false, string.Empty));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task ApproveAlbum_ReturnsFailure_WhenAlbumNotFound()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var request = new ApproveAlbumRequest(albumId);

        var mockDbSet = new Mock<DbSet<Album>>();
        mockDbSet.Setup(x => x.FindAsync(new object[] { albumId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Album?)null);
        _contextMock.Setup(x => x.Albums).Returns(mockDbSet.Object);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.NotFound, result.Error.Type);
    }

    [Fact]
    public async Task ApproveAlbum_ChangesStatusToActive_WhenAlbumIsNotApproved()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var album = new Album { Id = albumId, Name = "Test", Status = AlbumStatus.NotApproved };
        var request = new ApproveAlbumRequest(albumId);

        var mockDbSet = new Mock<DbSet<Album>>();
        mockDbSet.Setup(x => x.FindAsync(new object[] { albumId }, It.IsAny<CancellationToken>()))
            .ReturnsAsync(album);
        _contextMock.Setup(x => x.Albums).Returns(mockDbSet.Object);
        _contextMock.Setup(x => x.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(AlbumStatus.Active, album.Status);
        _contextMock.Verify(x => x.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
