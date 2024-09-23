using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.Album;
using CoinyProject.Application.DTO.Album;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Application.Services;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CoinyProject.UnitTests.Services;

public class AlbumServiceUnitTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AlbumService _albumService;

    public AlbumServiceUnitTests()
    {
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _albumService = new AlbumService(_mapperMock.Object, _unitOfWorkMock.Object, _httpContextAccessorMock.Object);
    }

    private ClaimsPrincipal GetUser(bool isAuthenticated, string userId)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId)
        };
        return new ClaimsPrincipal(new ClaimsIdentity(claims, isAuthenticated ? "auth" : ""));
    }

    [Fact]
    public async Task AddAlbumAsync_ThrowsUnauthorizedAccess_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var albumPostDto = new AlbumPostDto("test_name", "test_description");
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumService.AddAlbumAsync(albumPostDto));
    }

    [Fact]
    public async Task AddAlbumAsync_ReturnsAlbumId_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var albumPostDto = new AlbumPostDto("test_name", "test_description");
        var albumEntity = new Album { Id = Guid.NewGuid(), UserId = userId };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _mapperMock.Setup(x => x.Map<Album>(albumPostDto)).Returns(albumEntity);
        _unitOfWorkMock.Setup(x => x.Albums.AddAsync(albumEntity)).Returns(Task.CompletedTask);

        // Act
        var result = await _albumService.AddAlbumAsync(albumPostDto);

        // Assert
        Assert.Equal(albumEntity.Id, result);
        _unitOfWorkMock.Verify(x => x.Albums.AddAsync(albumEntity), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    /*[Fact]
    public async Task GetPagedAlbumsAsync_ThrowsNotFoundException_WhenNoAlbumsFound()
    {
        // Arrange
        var pagedAlbums = new PagedResponse<Album> { TotalPages = 0 };
        _unitOfWorkMock.Setup(x => x.Albums.GetPagedActiveAlbumsWithElementsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagedAlbums);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await _albumService.GetPagedAlbumsAsync(1, 10));
    }

    [Fact]
    public async Task GetPagedAlbumsAsync_ReturnsPagedAlbums_WhenFound()
    {
        // Arrange
        var pagedAlbums = new PagedResponse<Album> { TotalPages = 1, Items = new[] { new Album() } };
        var pagedAlbumDtos = new PagedResponse<AlbumGetDto>();

        _unitOfWorkMock.Setup(x => x.Albums.GetPagedActiveAlbumsWithElementsAsync(It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagedAlbums);
        _mapperMock.Setup(x => x.Map<PagedResponse<AlbumGetDto>>(pagedAlbums)).Returns(pagedAlbumDtos);

        // Act
        var result = await _albumService.GetPagedAlbumsAsync(1, 10);

        // Assert
        Assert.Equal(pagedAlbumDtos, result);
    }*/

    [Fact]
    public async Task GetAlbumById_ThrowsNotFoundException_WhenAlbumNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Album)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await _albumService.GetAlbumById(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAlbumById_ReturnsAlbumDto_WhenAlbumIsActiveAndUserIsOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var album = new Album { Id = Guid.NewGuid(), UserId = userId, Status = AlbumStatus.Active };
        var albumDto = new AlbumGetDto();

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(album);
        _mapperMock.Setup(x => x.Map<AlbumGetDto>(album)).Returns(albumDto);

        // Act
        var result = await _albumService.GetAlbumById(album.Id);

        // Assert
        Assert.Equal(albumDto, result);
    }

    [Fact]
    public async Task UpdateAlbumAsync_ThrowsUnauthorizedAccess_WhenUserIsNotOwner()
    {
        // Arrange
        var albumDto = new AlbumPatchDto("test_name", "test_description");
        var userId = Guid.NewGuid();
        var album = new Album { UserId = Guid.NewGuid() };
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(album);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => 
            await _albumService.UpdateAlbumAsync(Guid.NewGuid(), albumDto));
    }

    [Fact]
    public async Task DeactivateAlbumAsync_CallsChangeAlbumStatus_WithCorrectParameters()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var album = new Album { UserId = userId, Status = AlbumStatus.Active };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumId)).ReturnsAsync(album);

        // Act
        await _albumService.DeactivateAlbumAsync(albumId);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.Equal(AlbumStatus.Inactive, album.Status);
    }

      [Fact]
    public async Task ActivateAlbumAsync_ThrowsInvalidOperationException_WhenAlbumHasLessThan4Elements()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var pagedAlbumElements = new PagedResponse<AlbumElement> { Items = new[] { new AlbumElement(), new AlbumElement(), new AlbumElement() } };

        _unitOfWorkMock.Setup(x => x.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(albumId, 1, 10)).ReturnsAsync(pagedAlbumElements);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => await _albumService.ActivateAlbumAsync(albumId));
    }

    [Fact]
    public async Task ActivateAlbumAsync_ChangesAlbumStatusToNotApproved_WhenAlbumHas4OrMoreElements()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var pagedAlbumElements = new PagedResponse<AlbumElement> { Items = new[] { new AlbumElement(), new AlbumElement(), new AlbumElement(), new AlbumElement() } };
        var album = new Album { Id = albumId, UserId = userId, Status = AlbumStatus.Inactive };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(albumId, 1, 10)).ReturnsAsync(pagedAlbumElements);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumId)).ReturnsAsync(album);

        // Act
        await _albumService.ActivateAlbumAsync(albumId);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.Equal(AlbumStatus.NotApproved, album.Status);
    }

    [Fact]
    public async Task ApproveAlbumAsync_ThrowsNotFoundException_WhenAlbumNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Album)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await _albumService.ApproveAlbumAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ApproveAlbumAsync_ChangesAlbumStatusToActive_WhenAlbumIsNotApproved()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var album = new Album { Id = albumId, Status = AlbumStatus.NotApproved };

        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumId)).ReturnsAsync(album);

        // Act
        await _albumService.ApproveAlbumAsync(albumId);

        // Assert
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
        Assert.Equal(AlbumStatus.Active, album.Status);
    }

    [Fact]
    public async Task ChangeAlbumStatus_ThrowsUnauthorizedAccessException_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(false, string.Empty));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumService.DeactivateAlbumAsync(albumId));
    }

    [Fact]
    public async Task ChangeAlbumStatus_ThrowsUnauthorizedAccessException_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var album = new Album { UserId = Guid.NewGuid(), Status = AlbumStatus.Active };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(album);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumService.DeactivateAlbumAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task ChangeAlbumStatus_ChangesStatus_WhenConditionsAreMet()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var albumId = Guid.NewGuid();
        var album = new Album { Id = albumId, UserId = userId, Status = AlbumStatus.Active };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumId)).ReturnsAsync(album);

        // Act
        await _albumService.DeactivateAlbumAsync(albumId);

        // Assert
        Assert.Equal(AlbumStatus.Inactive, album.Status);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}