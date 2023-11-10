using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZATCA.API.Models.Customer
{
    public class CustomerVM
    {


        public string IdentityType { get; set; }
        public string IdentityNumber { get; set; }

        public string StreetName { get; set; }
        public string BuildingNumber { get; set; }
        public string CityName { get; set; }
        public string RegionName { get; set; }
        public string DistrictName { get; set; }

        public string AdditionalStreetAddress { get; set; }
        public string VatRegNumber { get; set; }
        public string ZipCode { get; set; }

        public string CustomerName { get; set; }
        [Required(ErrorMessage = "CountryCode is Required")]
        [RegularExpression("SA", ErrorMessage = "The CountryCode must be either 'SA' only.")]

        public string CountryCode { get; set; }
        //       public int InvoiceDataId { get; set; }

    }
}