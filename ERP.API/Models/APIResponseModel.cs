namespace ZATCA.API.Models
{
    public class APIResponseModel<T>
    {
        public bool IsError { get; set; }
        public string message { get; set; }
        public List<T> Data { get; set; }
    }
}
