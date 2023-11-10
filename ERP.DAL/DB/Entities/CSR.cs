using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class CSR : IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

        public string VatNumber { get; set; }
        public string TaxPayerName { get; set; }
        public string TaxPayerEmail { get; set; }
        public string InvoiceType { get; set; }
        public string Address { get; set; }
        public string BusinessCategory { get; set; }
        public string CSRFor { get; set; }

        public string CsrFile { get; set; }
        public string Cnf { get; set; }
        public string Pem { get; set; }

        public int CompanyId { get; set; }
        public Company Company { get; set; }
    }
}
