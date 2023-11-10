namespace ZATCA.API.Models
{
    public class ResponseAPIVM<T>
    {
        public bool IsError { get; set; }
        public string Message { get; set; }
        public T data { get; set; }
    }
}
