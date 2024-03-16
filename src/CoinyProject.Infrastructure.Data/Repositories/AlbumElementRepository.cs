using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories
{
    public class AlbumElementRepository : BaseRepository<AlbumElement>
    {
        public AlbumElementRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
        }
    }
}
