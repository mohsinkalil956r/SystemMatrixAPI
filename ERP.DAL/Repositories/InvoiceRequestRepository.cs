using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.DB;
using ZATCA.DAL.Repositories.Abstraction;
using ZATCA.DAL.Repositories;

namespace ZATCA.DAL.Repositories
{
    public class InvoiceRequestRepository : BaseRepository<InvoiceRequest>, IInvoiceRequestRepository
    {
        public InvoiceRequestRepository(ZatcaContext context)
           : base(context)
        {

        }
    }
}




