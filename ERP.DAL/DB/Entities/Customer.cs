using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class Customer
    {
        public int Id { get; set; }
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

        public string CountryCode { get; set; } = "SA";
        public int InvoiceDataId { get; set; }
        [NotMapped]
        public InvoiceData InvoiceData { get; set; }
    }
}
