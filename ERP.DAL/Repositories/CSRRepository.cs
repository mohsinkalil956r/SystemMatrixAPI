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
    public class CSRRepository : BaseRepository<CSR> , ICSRRepository
    {
        public CSRRepository(ZatcaContext context) 
            : base (context)
        {
            
        }
    }
}
