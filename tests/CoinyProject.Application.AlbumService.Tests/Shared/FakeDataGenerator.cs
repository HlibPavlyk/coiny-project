using Bogus;
using CoinyProject.Application.Features.Albums.Models;

namespace CoinyProject.UnitTests.Shared
{
    public class FakeDataGenerator
    {
        Faker<UpdateAlbumModel> inputAlbumFake;

        public FakeDataGenerator()
        {
            Randomizer.Seed = new Random(8675309);

            inputAlbumFake = new Faker<UpdateAlbumModel>()
                .WithRecord()
                .RuleFor(a => a.Name, f => f.Lorem.Word())
                .RuleFor(a => a.Description, f => f.Lorem.Sentence());
        }

        public UpdateAlbumModel GetInputAlbum()
        {
            return inputAlbumFake.Generate();
        }

        public string GetRandomUserId()
        {
            return Guid.NewGuid().ToString();
        }


    }

}
