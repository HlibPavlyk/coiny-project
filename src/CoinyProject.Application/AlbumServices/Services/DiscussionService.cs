using CoinyProject.Application.AlbumServices.Interfaces;
using CoinyProject.Infrastructure.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoinyProject.Application.AlbumServices.Services
{
    public class DiscussionService :IDiscussionService
    {
        private readonly ApplicationDBContext _dBContext;
        public DiscussionService(ApplicationDBContext dBContext)
        {
            _dBContext = dBContext;
        }
    }
}
