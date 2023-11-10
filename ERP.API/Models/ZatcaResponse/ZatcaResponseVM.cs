using ZATCA.API.Models.InvoiceRequest;
using ZATCA.API.Models.ValidationResult;

namespace ZATCA.API.Models.ZatcaResponse

{
    public class ZatcaResponseVM
    {
        public string ResponseXML { get; set; }

        public bool Isreported { get; set; }
        public string status { get; set; }

        public string InvoiceBase64 { get; set; }
        public string InvoiceHash { get; set; }

        public string QrCode { get; set; }

        public DateTime SubmissionDate { get; set; }
        public string WarnngMessage { get; set; }


        public string ErrorMessage { get; set; }

        public int InvoiceRequestId { get; set; }
        public string InfoMessage { get; set; }

        //public int ValidationResultId { get; set; }
        //// [ForeignKey("ValidationResultId")]
        //public ValidationResultVM ValidationResult { get; set; }
        ////// [ForeignKey("InvoiceRequestId")]
        public InvoiceRequestVM InvoiceRequest { get; set; }

    }
}
