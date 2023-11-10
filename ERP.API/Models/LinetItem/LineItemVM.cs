namespace ZATCA.API.Models.LinetItem
{
    public class LineItemVM
    {
        public string Id { get; set; }
        public int Index { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public double NetPrice { get; set; }
        public double LineDiscount { get; set; }
        public double PriceDiscount { get; set; }
        public int InvoiceDataId { get; set; }

        public double Tax { get; set; }
        public string TaxCategory { get; set; }
        public string TaxCategoryReasonCode { set; get; }
        public string TaxCategoryReason { set; get; }
    }
}