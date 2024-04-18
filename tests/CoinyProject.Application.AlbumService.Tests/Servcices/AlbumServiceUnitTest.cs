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
using Microsoft.AspNetCore.Routing;
using System.Reflection;
using static System.Formats.Asn1.AsnWriter;
using CoinyProject.UnitTests.Shared;
using AutoFixture;
using Microsoft.AspNetCore.Http;
using AutoFixture.Kernel;
using CoinyProject.Infrastructure.Data.Repositories.Interfaces;
using CoinyProject.Infrastructure.Data.Repositories.Realization;
using System.Xml.Linq;
using CoinyProject.Infrastructure.Data.Migrations;

namespace CoinyProject.UnitTests.Servcices
{
    public class AlbumServiceUnitTest
    {
        private readonly IMapper _mapper;
        private readonly Mock<IWebHostEnvironment> _webHostEnvironment;

        private readonly FakeDataGenerator _fakeDataGenerator;
        private readonly Fixture _fixture;


        public AlbumServiceUnitTest()
        {
            _webHostEnvironment = new();
            _fakeDataGenerator = new();
            _fixture = new();

            var configuration = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new AutoMapperProfile());
            });
            _mapper = new Mapper(configuration);
        }
        public async Task<Stream> GetTestImage()
        {
            var memoryStream = new MemoryStream();
            var fileStream = File.OpenRead("test.jpg");
            await fileStream.CopyToAsync(memoryStream);
            fileStream.Close();
            return memoryStream;
        }

        [Fact]
        public async Task AddAlbum_ReturnsAlbumId_IfAddingAlbumIsCorrect()
        {
            var albumCreating = _fakeDataGenerator.GetInputAlbum();
            var userId = _fakeDataGenerator.GetRandomUserId();
            var albumId = 1;

            List<Album> albumList = new();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(x => x.InsertAsync(It.IsAny<Album>()))
                .Callback((Album model) => { model.Id = albumId; albumList.Add(model); })
                .Returns(Task.CompletedTask);

            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            var result = await albumService.AddAlbum(albumCreating, userId);

            albumRepository.Verify(m => m.InsertAsync(It.IsAny<Album>()), Times.Once);
            unitOfWork.Verify(db => db.SaveChangesAsync(), Times.Once);
            result.Should().Be(albumId);
            albumList.Should().ContainSingle(
                a => a.Name == albumCreating.Name &&
                a.Description == albumCreating.Description
                && a.UserId == userId);
        }

        [Theory]
        [InlineData(null, null)]
        public async Task AddAlbum_ReturnsException_IfInputArgumentsAreNull(AlbumCreating? albumCreating, string? userId)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.AddAlbum(albumCreating, userId));

            albumRepository.Verify(m => m.InsertAsync(It.IsAny<Album>()), Times.Never);
            unitOfWork.Verify(db => db.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task AddAlbumElement_ReturnsNoErrors_IfAddingAlbumIsCorrect()
        {
            var image = await GetTestImage();
            _fixture.Customize<IFormFile>(c => c.FromFactory(() =>
                new FormFile(image, 0, image.Length, "file", "test.jpg")
            ));
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));

            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var element = _fixture.Build<AlbumElementCreating>()
                .Create();

            var album = _fixture.Build<Album>()
                .With(x => x.Elements, new List<AlbumElement>())
                .With(x => x.Id, element.AlbumId)
                .Create();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            _webHostEnvironment.Setup(x => x.WebRootPath).Returns("..\\..\\..\\test");

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithElementsById(It.IsAny<int>()))
                .ReturnsAsync(album);

            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await albumService.AddAlbumElement(element);

            albumRepository.Verify(m => m.GetAlbumWithElementsById(It.IsAny<int>()), Times.Once);
            unitOfWork.Verify(db => db.SaveChangesAsync(), Times.Once);
            album.Elements.Should().ContainSingle(
                a => a.Name == element.Name &&
                a.Description == element.Description);

        }

        [Theory]
        [InlineData(null)]
        public async Task AddAlbumElement_ReturnsException_IfInputArgumentsAreNull(AlbumElementCreating? albumCreating)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.AddAlbumElement(albumCreating));

            albumRepository.Verify(m => m.GetAlbumWithElementsById(It.IsAny<int>()), Times.Never);
            unitOfWork.Verify(db => db.SaveChangesAsync(), Times.Never);
        }

        [Fact]
        public async Task GetAllAlbumsDTO_ReturnsNoErrors_IfCorrect()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            IEnumerable<Album> albums = _fixture.CreateMany<Album>(5);
            var testedAlbum = albums.First();
            
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAllAlbumsWithElements(It.IsAny<string>()))
                .ReturnsAsync(albums);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            var result = await albumService.GetAllAlbumsDTO(testedAlbum.UserId);

            albumRepository.Verify(m => m.GetAllAlbumsWithElements(It.IsAny<string>()), Times.Once);
            result.Should().HaveCount(5);
            result.First().Should().BeEquivalentTo(
                new AlbumGetDTO(
                    testedAlbum.Id,
                    testedAlbum.Name,
                    testedAlbum.Description,
                    testedAlbum.Rate,
                    testedAlbum.Elements.First().ImageURL)
             );
        }

        [Theory]
        [InlineData(null)]
        public async Task GetAllAlbumsDTO_ReturnsException_IfInputArgumentsAreNull(string? userId)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.GetAllAlbumsDTO(userId));

            albumRepository.Verify(m => m.GetAllAlbumsWithElements(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAllAlbumsDTOForView_ReturnsNoErrors_IfCorrect()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            IEnumerable<Album> albums = _fixture.CreateMany<Album>(5);
            var testedAlbum = albums.First();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAllAlbumsWithElementsAndFavoritesForView(It.IsAny<string>()))
                .ReturnsAsync(albums);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            var result = await albumService.GetAllAlbumsForView(testedAlbum.UserId);

            albumRepository.Verify(m => m.GetAllAlbumsWithElementsAndFavoritesForView(It.IsAny<string>()), Times.Once);
            result.Should().HaveCount(5);
            result.First().Should().BeEquivalentTo(
                new AlbumGetForViewDTO()
                {
                    Id = testedAlbum.Id,
                    Name = testedAlbum.Name,
                    Description = testedAlbum.Description,
                    Rate = testedAlbum.Rate,
                    TitleImageURL = testedAlbum.Elements.First().ImageURL,
                    IsFavorite = false
                });
        }

        [Theory]
        [InlineData(null)]
        public async Task GetAllAlbumsForView_ReturnsException_IfInputArgumentsAreNull(string? userId)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.GetAllAlbumsForView(userId));

            albumRepository.Verify(m => m.GetAllAlbumsWithElementsAndFavoritesForView(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetAlbumById_ReturnsNoErrors_IfCorrect()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var album = _fixture.Create<Album>();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithElementsById(It.IsAny<int>()))
                .ReturnsAsync(album);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            var result = await albumService.GetAlbumById(album.Id);

            

            albumRepository.Verify(m => m.GetAlbumWithElementsById(It.IsAny<int>()), Times.Once);
            result.Should().BeEquivalentTo(
                new AlbumGetByIdDTO(
                    album.Id,
                    album.Name,
                    album.Description,
                    album.Rate,
                    album.UserId,
                    album.Elements.Select(element => new AlbumElementGetDTO(
                        element.Id,
                        element.Name,
                        element.Description,
                        element.ImageURL
                        )).ToList()
                ));
        }

        [Theory]
        [InlineData(null)]
        public async Task GetAlbumById_ReturnsException_IfInputArgumentsAreNull(int? id)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.GetAlbumById(id));

            albumRepository.Verify(m => m.GetAlbumWithElementsById(It.IsAny<int>()), Times.Never);
        }

        [Fact]
        public async Task GetAlbumForEdit_ReturnsNoErrors_IfCorrect()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var album = _fixture.Create<Album>();
            var currentUserId = album.UserId;

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((int albumId, string userId) =>
                {
                    if(album.UserId == userId)
                    {
                        return album;
                    }
                    return null;
                });
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            var result = await albumService.GetAlbumForEdit(album.Id, currentUserId);

            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            result.Should().BeEquivalentTo(
            new AlbumEditDTO(
                album.Id,
                album.Name,
                album.Description
                ));
        }

        [Fact]
        public async Task GetAlbumForEdit_ReturnsException_IfCurrentUserIsNotAuthor()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var album = _fixture.Create<Album>();
            var currentUserId = _fixture.Create<string>();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((int albumId, string userId) =>
                {
                    if (album.UserId == userId)
                    {
                        return album;
                    }
                    return null;
                });
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.GetAlbumForEdit(album.Id, currentUserId));

            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
        }

        [Theory]
        [InlineData(null, null)]
        public async Task GetAlbumForEdit_ReturnsException_IfInputArgumentsAreNull(int? id, string? currentUserId)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.GetAlbumForEdit(id, currentUserId));

            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task UpdateAlbum_ReturnsNoErrors_IfCorrect()
        {
            _fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => _fixture.Behaviors.Remove(b));
            _fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var album = _fixture.Create<Album>();
            var albumEdit = _fixture.Create<AlbumEditDTO>();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumById(It.IsAny<int>()))
                .ReturnsAsync(album);
            albumRepository.Setup(a => a.Update(It.IsAny<Album>()))
                .Callback((Album model) => { album.Name = albumEdit.Name; album.Description = albumEdit.Description; });

            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await albumService.UpdateAlbum(albumEdit);

            albumRepository.Verify(m => m.GetAlbumById(It.IsAny<int>()), Times.Once);
            albumRepository.Verify(m => m.Update(It.IsAny<Album>()), Times.Once);
            unitOfWork.Verify(db => db.SaveChanges(), Times.Once);
            albumEdit.Should().BeEquivalentTo(
                new AlbumEditDTO(
                    album.Id,
                    album.Name,
                    album.Description
                 ));
        }

        [Theory]
        [InlineData(null)]
        public async Task UpdateAlbum_ReturnsException_IfInputArgumentsAreNull(AlbumEditDTO? album)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.UpdateAlbum(album));

            albumRepository.Verify(m => m.GetAlbumById(It.IsAny<int>()), Times.Never);
            albumRepository.Verify(m => m.Update(It.IsAny<Album>()), Times.Never);
            unitOfWork.Verify(db => db.SaveChanges(), Times.Never);
        }

        [Fact]
        public async Task DeleteAlbum_ReturnsNoErrors_IfCorrect()
        {
            var album = _fixture.Build<Album>()
                .Without(x => x.Elements)
                .Without(x => x.User)
                .Without(x => x.FavoriteAlbums)
                .Create();
            var currentUserId = album.UserId;

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((int albumId, string userId) =>
                {
                    if (album.UserId == userId)
                    {
                        return album;
                    }
                    return null;
                });
            albumRepository.Setup(a => a.Delete(It.IsAny<Album>()))
                .Callback((Album model) => { album = null; });

            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await albumService.DeleteAlbum(album.Id, currentUserId);

            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            albumRepository.Verify(m => m.Delete(It.IsAny<Album>()), Times.Once);
            unitOfWork.Verify(db => db.SaveChanges(), Times.Once);
            album.Should().BeNull();
        }


        [Fact]
        public async Task DeleteAlbum_ReturnsException_IfCurrentUserIsNotAuthor()
        {
            var album = _fixture.Build<Album>()
                .Without(x => x.Elements)
                .Without(x => x.User)
                .Without(x => x.FavoriteAlbums)
                .Create();
            var currentUserId = _fixture.Create<string>();

            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);

            albumRepository.Setup(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()))
                .ReturnsAsync((int albumId, string userId) =>
                {
                    if (album.UserId == userId)
                    {
                        return album;
                    }
                    return null;
                });
            albumRepository.Setup(a => a.Delete(It.IsAny<Album>()))
                .Callback((Album model) => { album = null; });

            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.DeleteAlbum(album.Id, currentUserId));

            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Once);
            albumRepository.Verify(m => m.Delete(It.IsAny<Album>()), Times.Never);
            unitOfWork.Verify(db => db.SaveChanges(), Times.Never);
        }

        [Theory]
        [InlineData(null, null)]
        public async Task DeleteAlbum_ReturnsException_IfInputArgumentsAreNull(int? id, string? currentUserId)
        {
            Mock<IUnitOfWork> unitOfWork = new();
            Mock<IAlbumRepository> albumRepository = new();

            unitOfWork.Setup(Album => Album.Albums).Returns(albumRepository.Object);
            var albumService = new AlbumService(_webHostEnvironment.Object, _mapper, unitOfWork.Object);

            await Assert.ThrowsAsync<ArgumentNullException>(async () => await albumService.DeleteAlbum(id, currentUserId));
            
            albumRepository.Verify(m => m.GetAlbumWithAuthorCheck(It.IsAny<int>(), It.IsAny<string>()), Times.Never);
            albumRepository.Verify(m => m.Delete(It.IsAny<Album>()), Times.Never);
            unitOfWork.Verify(db => db.SaveChanges(), Times.Never);
                   
        }


    }
}
