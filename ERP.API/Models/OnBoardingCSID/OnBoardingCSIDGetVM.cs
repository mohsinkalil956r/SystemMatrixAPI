namespace ZATCA.API.Models.OnBoardingCSID
{
    public class OnBoardingCSIDGetVM
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        public string UserName { get; set; }
        public string Secret { get; set; }
        public DateTime StartedDate { get; set; }
        public DateTime ExpiredDate { get; set; }

    }
}
