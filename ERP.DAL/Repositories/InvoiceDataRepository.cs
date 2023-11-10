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
    public class InvoiceDataRepository: BaseRepository<InvoiceData>, IInvoiceDataRepository
    {
         public InvoiceDataRepository(ZatcaContext context)
            : base(context)
            {

            }
    }
}




