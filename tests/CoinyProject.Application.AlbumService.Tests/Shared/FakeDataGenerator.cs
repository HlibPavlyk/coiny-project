using Bogus;
using Castle.DynamicProxy;
using CoinyProject.Application.DTO.Album;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using CoinyProject.Application.Dto.Album;

namespace CoinyProject.UnitTests.Shared
{
    public class FakeDataGenerator
    {
        Faker<AlbumPostDto> inputAlbumFake;

        public FakeDataGenerator()
        {
            Randomizer.Seed = new Random(8675309);

            inputAlbumFake = new Faker<AlbumPostDto>()
                .WithRecord()
                .RuleFor(a => a.Name, f => f.Lorem.Word())
                .RuleFor(a => a.Description, f => f.Lorem.Sentence());
        }

        public AlbumPostDto GetInputAlbum()
        {
            return inputAlbumFake.Generate();
        }

        public string GetRandomUserId()
        {
            return Guid.NewGuid().ToString();
        }


    }

}
