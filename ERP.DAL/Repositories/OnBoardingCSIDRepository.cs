using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZATCA.DAL.DB;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;

namespace ZATCA.DAL.Repositories
{
    public class OnBoardingCSIDRepository : BaseRepository<OnBoardingCSID> ,IOnBoardingCSIDRepository
    {
        public OnBoardingCSIDRepository(ZatcaContext context) : base(context)
        {
            
        }
    }
}
