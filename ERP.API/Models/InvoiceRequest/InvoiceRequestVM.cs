
using ZATCA.API.Models.ZatcaResponse;
using ZATCA.DAL.DB.Entities;

namespace ZATCA.API.Models.InvoiceRequest
{
    public class InvoiceRequestVM
    {
        public int Id { get; set; }
        public string Detail { get; set; }

        public int Status { get; set; }

        public int CreatedBy { get; set; }
        public DateTime RequestDate { get; set; }


        public List<ZatcaResponseVM> Responses { get; set; } = new();

    }
}
