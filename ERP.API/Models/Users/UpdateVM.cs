using System.ComponentModel.DataAnnotations;

namespace ZATCA.API.Models.Users
{
    public class UpdateVM
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string UserType { get; set; }

        public string PhoneNumber { get; set; }

        public bool isActive { get; set; }

        public List<string> Permissions { get; set; }

        public int CompanyId { get; set; }

    }
}
