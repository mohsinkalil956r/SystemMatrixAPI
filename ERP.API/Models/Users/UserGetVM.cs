namespace ZATCA.API.Models.Users
{
    public class UserGetVM
    {
        public int Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public int CompanyId { get; set; }
        public string UserType { get; set; }
        public bool IsActive { get; set; }
        public bool RequiredPasswordReset { get; set; }
        public List<string> Roles { get; set; }
        public List<string> Permissions { get; set; }

    }
}
