using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZATCA.API.Models.Supplier
{
    public class SupplierVM
    {
        [Required(ErrorMessage = "SellerTRN is Required")]
        public string SellerTRN { get; set; }

        [Required(ErrorMessage = "SellerName is Required")]
        public string SellerName { get; set; }

        public string StreetName { get; set; }
        public string CityName { get; set; }
        public string DistrictName { get; set; }
        [Required(ErrorMessage = "IdentityType  is Required")]

        public string BuildingNumber { get; set; }
        [RegularExpression("CRN|TIN|PAS", ErrorMessage = "The IdentityType must be either 'NAT' or 'TIN' or 'PAS'only.")]

        [Required(ErrorMessage = "IdentityType  is Required")]
        public string IdentityType { get; set; }
        [Required(ErrorMessage = "IdentityNumber is Required")]
        public string IdentityNumber { get; set; }
        [Required(ErrorMessage = "CountryCode is Required")]
        [RegularExpression("SA", ErrorMessage = "The CountryCode must be either 'SA' only.")]

        public string CountryCode { get; set; }

        public string AdditionalStreetAddress { get; set; }
        [MinLength(5)]
        public string PostalCode { get; set; }
        //    public int InvoiceDataId { get; set; }

    }
}