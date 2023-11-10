using ZATCA.DAL.DB;
using ZATCA.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.Repositories
{
    public class UnitOfWorkRepository : IUnitOfWork
    {
        private readonly ZatcaContext _context;

        public UnitOfWorkRepository(ZatcaContext context)
        {
            this._context = context;
        }

        private UserRepository _userRepository;

        public UserRepository UserRepository
        {
            get
            {
                if (_userRepository == null)
                {
                    this._userRepository = new UserRepository(this._context);
                }
                return _userRepository;
            }
        }

        public async Task CommitAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
