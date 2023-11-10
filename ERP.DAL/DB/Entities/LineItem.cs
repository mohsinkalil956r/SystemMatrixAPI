
using System.ComponentModel.DataAnnotations.Schema;

namespace ZATCA.DAL.DB.Entities
{
    public class LineItem
    {
        public LineItem()
        {
            Id = Guid.NewGuid().ToString();
        }
        public string Id { get; set; }
        public int Index { get; set; }
        public string ProductName { get; set; }
        public double Quantity { get; set; }
        public double NetPrice { get; set; }
        public double LineDiscount { get; set; }
        public double PriceDiscount { get; set; }
        public int InvoiceDataId { get; set; }
        [NotMapped]
        public InvoiceData InvoiceData { get; set; }
        /// <summary>
        /// The line VAT amount (KSA-11) must be Invoice line net amount (BT-131) x(Line VAT rate (BT152)/100)
        /// </summary>

        public double TaxAmount
        {
            get
            {
                return double.Parse((TotalWithoutTax * Tax / 100).ToString("0.00"));
            }
        }
        public double TotalWithTax
        {
            get
            {
                return double.Parse((TaxAmount + TotalWithoutTax).ToString("0.00"));
            }
        }

        /// <summary>
        /// The invoice line net amount without VAT, and inclusive of line level allowance.
        /// Item line net amount (BT-131) = ((Item net price (BT-146) ÷ Item price base quantity(BT-149)) 
        ///     × (Invoiced Quantity (BT-129)) − Invoice line allowance amount(BT-136)
        /// </summary>
        public double TotalWithoutTax
        {
            get
            {
                return double.Parse(Math.Round((float)(Quantity * NetPrice - LineDiscount), 3).ToString("0.00"));
            }
        }

        public double GrossPrice
        {
            get
            {
                return (NetPrice + PriceDiscount);
            }
        }
        public double Tax { get; set; }
        public string TaxCategory { get; set; } = "S";
        public string TaxCategoryReasonCode { set; get; }
        public string TaxCategoryReason { set; get; }

    }
}