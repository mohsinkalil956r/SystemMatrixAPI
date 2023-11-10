using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class ValidationResult:IBaseEntity
    {


        public int Id { get; set; }
        public bool IsActive { get; set; } = true;

    //    public int InfoMessageId { get; set; }
      
        //public int WarnngMessageId { get; set; }
        //public int ErrorMessageId { get; set; }
        public string status { get; set; }

        public string InvoiceBase64 { get; set; }
        public string  InvoiceHash { get; set; }

        public string QrCode { get; set; }

        public DateTime SubmissionDate { get; set; }
        public string WarnngMessage { get; set; } 
       
       
        public string ErrorMessage { get; set; }

       
        public string InfoMessage { get; set; } 
    }
}
