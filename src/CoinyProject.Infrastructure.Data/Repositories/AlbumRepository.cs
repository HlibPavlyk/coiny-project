using CoinyProject.Core.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Infrastructure.Data.Repositories
{
    public class AlbumRepository : BaseRepository<Album>
    {
        public AlbumRepository(ApplicationDBContext dBContext) : base(dBContext)
        {
        }

        public ApplicationDBContext ApplicationDBContex
        {
            get
            {
                return _dbContext as ApplicationDBContext;
            }
        }
    }
}
