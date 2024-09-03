using CoinyProject.Application.Abstractions.Repositories;
using CoinyProject.Domain.Entities;

namespace CoinyProject.Infrastructure.Repositories
{
    public class AlbumElementRepository : GenericRepository<AlbumElement>, IAlbumElementRepository
    {
        public AlbumElementRepository(ApplicationDbContext context) : base(context) {}

    }
}
