using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class ZatcaResponse : IBaseEntity
    {
        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

        public string ResponseXML { get; set; }

        public bool Isreported { get; set; }
        public string status { get; set; }

        public string InvoiceBase64 { get; set; }
        public string InvoiceHash { get; set; }

        public string QrCode { get; set; }

        public DateTime SubmissionDate { get; set; }
        public string WarnngMessage { get; set; }


        public string ErrorMessage { get; set; }


        public string InfoMessage { get; set; }

     //   public int ValidationResultId { get; set; }
        // [ForeignKey("ValidationResultId")]
     //   public ValidationResult ValidationResult { get; set; }
        // [ForeignKey("InvoiceRequestId")]
        public InvoiceRequest InvoiceRequest { get; set; }
        //ValidationResultMessag
    }
}
