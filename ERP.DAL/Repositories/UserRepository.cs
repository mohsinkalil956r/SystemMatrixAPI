using ZATCA.DAL.DB;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.Repositories
{
    public class UserRepository : BaseRepository<SystemUser>, IUserRepository
    {
        private readonly ZatcaContext _context;
        public UserRepository(ZatcaContext context) : base(context)
        {
            _context = context;
        }
    }
}
