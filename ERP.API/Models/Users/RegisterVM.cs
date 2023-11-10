using System.ComponentModel.DataAnnotations;

namespace ZATCA.API.Models.Users
{
    public class RegisterVM : UpdateVM
    {
        

        [Required]
        [EmailAddress]
        public string Email { get; set; }

    }
}
