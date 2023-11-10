using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class InvoiceResponse : IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string Response { get; set; }
        public int InvoiceRequestId { get; set; }
        // [ForeignKey("InvoiceRequestId")]
        public InvoiceRequest InvoiceRequest { get; set; }
    }
}
