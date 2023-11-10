namespace ZATCA.API.Models.Company
{
    public class CompanyPostVM
    {
        public string Name { get; set; }
        public string Email { get; set; }
        public string BusinessCategory { get; set; }
        public string Number { get; set; }
        public string Token { get; set; }
        public string CountryCode { get; set; }
        public string VatNumber { get; set; }
        public string Industry { get; set; }
        public string UnitName { get; set; }
        public string ArchiveType { get; set; }
        public string Environment { get; set; }
        public string ArchiveConfig { get; set; }
        public DateTime ContractStart { get; set; }
        public DateTime ContractEnd { get; set; }
        public string Address { get; set; }
        public bool CompanyIsActive { get; set; }

        public string AdditionalStreetAddress { get; set; }
        public string BuildingNumber { get; set; }
        public string CityName { get; set; }
        public string IdentityNumber { get; set; }
        public string IdentityType { get; set; }
        public string DistrictName { get; set; }
        public string PostalCode { get; set; }
        public string StreetName { get; set; }

    }
}
