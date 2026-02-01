using AutoMapper;
using CoinyProject.Application.Abstractions.Data;
using CoinyProject.Application.Abstractions.Identity;
using CoinyProject.Application.Common.Results;
using CoinyProject.Application.Features.Albums.Handlers;
using CoinyProject.Application.Features.Albums.Models;
using CoinyProject.Application.Features.Albums.Requests;
using Moq;

namespace CoinyProject.UnitTests.Services;

public class AlbumsHandlerUnitTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IApplicationDbContext> _contextMock;
    private readonly Mock<IIdentityService> _identityServiceMock;
    private readonly AlbumsHandler _handler;

    public AlbumsHandlerUnitTests()
    {
        _mapperMock = new Mock<IMapper>();
        _contextMock = new Mock<IApplicationDbContext>();
        _identityServiceMock = new Mock<IIdentityService>();
        _handler = new AlbumsHandler(_contextMock.Object, _mapperMock.Object, _identityServiceMock.Object);
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

        _identityServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("User is not authenticated")));

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

        _identityServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("User is not authenticated")));

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

        _identityServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("User is not authenticated")));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }

    [Fact]
    public async Task ActivateAlbum_ReturnsFailure_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var request = new ActivateAlbumRequest(albumId);

        _identityServiceMock.Setup(x => x.GetCurrentUserId())
            .Returns(Result.Failure<Guid>(Error.Unauthorized("User is not authenticated")));

        // Act
        var result = await _handler.Handle(request, CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(ErrorType.Unauthorized, result.Error.Type);
    }
}
