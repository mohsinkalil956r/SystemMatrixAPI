namespace ZATCA.API.Models
{
    public class PaginatedResult<T>
    {

        public List<T> Items { get; set; }
        public int TotalItems { get; set; }

        public PaginatedResult(List<T> items, int totalItems) {

            Items = items;
            TotalItems = totalItems;
        }

    }
}
