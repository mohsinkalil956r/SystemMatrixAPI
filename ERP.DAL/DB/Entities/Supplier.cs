using System.ComponentModel.DataAnnotations.Schema;

namespace ZATCA.DAL.DB.Entities
{
    public class Supplier
    {
        public int Id { get; set; }
        public string SellerTRN { get; set; }
        public string SellerName { get; set; }

        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string DistrictName { get; set; }

        public string BuildingNumber { get; set; }

        public string IdentityType { get; set; } 

        public string IdentityNumber { get; set; }

        public string CountryCode { get; set; } 

        public string AdditionalStreetAddress { get; set; }

        public string PostalCode { get; set; }
        public int InvoiceDataId { get; set; }
        [NotMapped]
        public InvoiceData InvoiceData { get; set; }
    }
}