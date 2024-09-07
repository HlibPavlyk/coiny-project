using System.Linq.Expressions;
using System.Security.Claims;
using AutoMapper;
using CoinyProject.Application.Abstractions.DataServices;
using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Application.Dto.AlbumElement;
using CoinyProject.Application.Dto.Other;
using CoinyProject.Application.Services;
using CoinyProject.Domain.Entities;
using CoinyProject.Domain.Enums;
using CoinyProject.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Moq;

namespace CoinyProject.UnitTests.Services;

public class AlbumElementServiceUnitTests
{
    private readonly Mock<IMapper> _mapperMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<IFileService> _fileServiceMock;
    private readonly Mock<IHttpContextAccessor> _httpContextAccessorMock;
    private readonly AlbumElementService _albumElementService;

    public AlbumElementServiceUnitTests()
    {
        _mapperMock = new Mock<IMapper>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _fileServiceMock = new Mock<IFileService>();
        _httpContextAccessorMock = new Mock<IHttpContextAccessor>();
        _albumElementService = new AlbumElementService(_mapperMock.Object, _unitOfWorkMock.Object, _fileServiceMock.Object, _httpContextAccessorMock.Object);
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
    public async Task AddAlbumElement_ThrowsUnauthorizedAccess_WhenUserIsNotAuthenticated()
    {
        // Arrange
        var element = new AlbumElementPostDto ("test_name", "test_description",Guid.NewGuid(),new FormFile(null, 0, 0, "photo", "photo.jpg"));
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(new ClaimsPrincipal());

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumElementService.AddAlbumElement(element));
    }

    [Fact]
    public async Task AddAlbumElement_ThrowsArgumentNullException_WhenAlbumDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var element = new AlbumElementPostDto ("test_name", "test_description",Guid.NewGuid(),new FormFile(null, 0, 0, "photo", "photo.jpg"));

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.AnyAsync(It.IsAny<Expression<Func<Album, bool>>>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _albumElementService.AddAlbumElement(element));
    }

    [Fact]
    public async Task AddAlbumElement_ReturnsElementId_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var emptyStream = new MemoryStream();
        var formFile = new FormFile(emptyStream, 0, emptyStream.Length, "photo", "photo.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg",
            ContentDisposition = "form-data; name=\"photo\"; filename=\"photo.jpg\""
        };
    
        var elementDto = new AlbumElementPostDto("test_name", "test_description", Guid.NewGuid(), formFile);
        var album = new Album { Id = elementDto.AlbumId, UserId = userId, Status = AlbumStatus.Active };
        var albumElement = new AlbumElement { Id = Guid.NewGuid(), AlbumId = album.Id, ImageUrl = "imageUrl.jpg" };

        // Set up mocks
        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.Albums.AnyAsync(It.IsAny<Expression<Func<Album, bool>>>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(elementDto.AlbumId)).ReturnsAsync(album);
        _unitOfWorkMock.Setup(x => x.AlbumElements.AddAsync(albumElement)).Returns(Task.CompletedTask);
        _mapperMock.Setup(x => x.Map<AlbumElement>(elementDto)).Returns(albumElement);
        _fileServiceMock.Setup(x => x.SaveImageAsync(elementDto.Photo)).ReturnsAsync("imageUrl.jpg");

        // Act
        var result = await _albumElementService.AddAlbumElement(elementDto);

        // Assert
        Assert.Equal(albumElement.Id, result);
        _unitOfWorkMock.Verify(x => x.AlbumElements.AddAsync(albumElement), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }


    [Fact]
    public async Task GetPagedAlbumElementsByAlbumIdAsync_ThrowsArgumentNullException_WhenAlbumNotFound()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        _unitOfWorkMock.Setup(x => x.Albums.AnyAsync(It.IsAny<Expression<Func<Album, bool>>>())).ReturnsAsync(false);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentNullException>(async () => await _albumElementService.GetPagedAlbumElementsByAlbumIdAsync(albumId, 1, 10));
    }

    [Fact]
    public async Task GetPagedAlbumElementsByAlbumIdAsync_ReturnsPagedAlbumElements_WhenSuccessful()
    {
        // Arrange
        var albumId = Guid.NewGuid();
        var pagedElements = new PagedResponse<AlbumElement> { TotalPages = 1, Items = new[] { new AlbumElement() } };
        var pagedElementDtos = new PagedResponse<AlbumElementGetDto>();

        _unitOfWorkMock.Setup(x => x.Albums.AnyAsync(It.IsAny<Expression<Func<Album, bool>>>())).ReturnsAsync(true);
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetPagedAlbumElementsByAlbumIdAsync(albumId, It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(pagedElements);
        _mapperMock.Setup(x => x.Map<PagedResponse<AlbumElementGetDto>>(pagedElements)).Returns(pagedElementDtos);

        // Act
        var result = await _albumElementService.GetPagedAlbumElementsByAlbumIdAsync(albumId, 1, 10);

        // Assert
        Assert.Equal(pagedElementDtos, result);
    }

    [Fact]
    public async Task GetAlbumElementByIdAsync_ThrowsNotFoundException_WhenElementNotFound()
    {
        // Arrange
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync((AlbumElement)null!);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(async () => await _albumElementService.GetAlbumElementByIdAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task GetAlbumElementByIdAsync_ReturnsElement_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var elementId = Guid.NewGuid();
        var albumElement = new AlbumElement { Id = elementId, AlbumId = Guid.NewGuid() ,};
        var album = new Album {Id  = albumElement.AlbumId, UserId = userId };
        var elementDto = new AlbumElementGetDto();

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(elementId)).ReturnsAsync(albumElement);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumElement.AlbumId)).ReturnsAsync(album);
        _mapperMock.Setup(x => x.Map<AlbumElementGetDto>(albumElement)).Returns(elementDto);

        // Act
        var result = await _albumElementService.GetAlbumElementByIdAsync(elementId);

        // Assert
        Assert.Equal(elementDto, result);
    }

    [Fact]
    public async Task UpdateAlbumElementAsync_ThrowsUnauthorizedAccess_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var elementDto = new AlbumElementPatchDto ("test_name", "test_description", new FormFile(null, 0, 0, "photo", "photo.jpg"));
        var albumElement = new AlbumElement { AlbumId = Guid.NewGuid() };
        var album = new Album { UserId = Guid.NewGuid() }; // Different user ID

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(albumElement);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumElement.AlbumId)).ReturnsAsync(album);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumElementService.UpdateAlbumElementAsync(Guid.NewGuid(), elementDto));
    }

    [Fact]
    public async Task UpdateAlbumElementAsync_UpdatesElement_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var elementDto = new AlbumElementPatchDto ("test_name", "test_description", new FormFile(null, 0, 0, "photo", "photo.jpg"));
        var albumElement = new AlbumElement { Id = Guid.NewGuid(), AlbumId = Guid.NewGuid(), ImageUrl = "oldUrl.jpg" };
        var album = new Album { UserId = userId };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(albumElement.Id)).ReturnsAsync(albumElement);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumElement.AlbumId)).ReturnsAsync(album);
        _fileServiceMock.Setup(x => x.SaveImageAsync(elementDto.Photo!)).ReturnsAsync("newImageUrl.jpg");

        // Act
        var result = await _albumElementService.UpdateAlbumElementAsync(albumElement.Id, elementDto);

        // Assert
        Assert.Equal(albumElement.Id, result);
        Assert.Equal("newImageUrl.jpg", albumElement.ImageUrl);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAlbumElementAsync_ThrowsUnauthorizedAccess_WhenUserIsNotOwner()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var albumElement = new AlbumElement { AlbumId = Guid.NewGuid() };
        var album = new Album { UserId = Guid.NewGuid() }; // Different user ID

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(It.IsAny<Guid>())).ReturnsAsync(albumElement);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumElement.AlbumId)).ReturnsAsync(album);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(async () => await _albumElementService.DeleteAlbumElementAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAlbumElementAsync_DeletesElement_WhenSuccessful()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var albumElement = new AlbumElement { Id = Guid.NewGuid(), AlbumId = Guid.NewGuid() };
        var album = new Album { UserId = userId };

        _httpContextAccessorMock.Setup(x => x.HttpContext.User).Returns(GetUser(true, userId.ToString()));
        _unitOfWorkMock.Setup(x => x.AlbumElements.GetByIdAsync(albumElement.Id)).ReturnsAsync(albumElement);
        _unitOfWorkMock.Setup(x => x.Albums.GetByIdAsync(albumElement.AlbumId)).ReturnsAsync(album);

        // Act
        await _albumElementService.DeleteAlbumElementAsync(albumElement.Id);

        // Assert
        _unitOfWorkMock.Verify(x => x.AlbumElements.Remove(albumElement), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
    }
}