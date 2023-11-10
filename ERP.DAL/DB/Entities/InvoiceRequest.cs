using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class InvoiceRequest : IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;
        public string Detail { get; set; }

        public int Status { get; set; }
        
        public int CreatedBy { get; set; }
        public DateTime RequestDate { get; set; }

       
        public List<ZatcaResponse> ZatcaResponses { get; set; } = new();

        //    public Company Company { get; set; }
    }
}
