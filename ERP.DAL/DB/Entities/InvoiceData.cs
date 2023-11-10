using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class InvoiceData
    {

        //Inv/2023/2/131231
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// GUID / UUID
        /// </summary>
        public string Id { get; set; }

        public int InvoiceType { get; set; }

        public int InvoiceTypeCode { get; set; }

        public string TransactionTypeCode { get; set; }

        public string Notes { get; set; }
        public int Order { get; set; }

        public string IssueDate { get; set; }

        public string IssueTime { get; set; }

        public string PreviousInvoiceHash { get; set; }

        public List<LineItem> Lines { get; set; } = new();
        public Supplier Supplier { get; set; }

        public double Discount { get; set; }

        public string ReferenceId { get; set; }

        public int PaymentMeansCode { get; set; } = 10;


        /// <summary>
        /// VAT category taxable amount (BT-116) = ∑(Invoice line net amounts (BT-113)) − Document level allowance amount (BT-93)
        /// VAT category tax amount(BT-117) = VAT category taxable amount(BT-116) × (VAT rate (BT-119) ÷ 100)
        /// </summary>
        public double TaxAmount
        {
            get
            {
                var lineNetAmounts = double.Parse((this.Lines.Sum(l => l.TaxCategory == "S" ? l.TotalWithoutTax : 0)).ToString("0.00"));
                return double.Parse(((lineNetAmounts > 0 ? (lineNetAmounts - Discount) : 0) * Tax / 100).ToString("0.00"));
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
        /// Invoice total amount without VAT (BT109) = Σ Invoice line net amount (BT-131) - Sum of allowances on document level (BT-107)
        /// Item line net amount (BT-131) = ((Item net price (BT-146) ÷ Item price base quantity(BT-149)) 
        ///     × (Invoiced Quantity (BT-129)) −Invoice line allowance amount(BT136)
        /// </summary>
        public double TotalWithoutTax
        {
            get
            {
                return double.Parse((this.Lines.Sum(l => l.TotalWithoutTax) - Discount).ToString("0.00"));
            }
        }

        public double TotalWithoutTaxAndDiscount
        {
            get
            {
                return double.Parse(this.Lines.Sum(l => l.TotalWithoutTax).ToString("0.00"));
            }
        }

        public int LinesCount
        {
            get { return this.Lines.Count; }
        }

        public Customer Customer { get; set; }

        public string DeliveryDate { get; set; }

        public double Tax { get; set; } = 15;

        public List<TaxSubTotal> SubTotals
        {
            get
            {
                var totals = this.Lines.GroupBy(l => l.TaxCategory, (c, r) => new TaxSubTotal
                {
                    TaxCategory = c,
                    Tax = r.FirstOrDefault().Tax,
                    TaxAmount = double.Parse((double.Parse((r.Sum(l => l.TotalWithoutTax) - double.Parse((c == DiscountTaxCategory ? Discount : 0).ToString("0.00"))).ToString("0.00")) * r.FirstOrDefault().Tax / 100).ToString("0.00")),
                    TotalWithoutTax = double.Parse(((r.Sum(l => l.TotalWithoutTax)) - double.Parse((c == DiscountTaxCategory ? Discount : 0).ToString("0.00"))).ToString("0.00")),
                    TaxCategoryReason = r.FirstOrDefault().TaxCategoryReason,
                    TaxCategoryReasonCode = r.FirstOrDefault().TaxCategoryReasonCode
                }).ToList();
                return totals;
            }
        }

        public string DiscountTaxCategory
        {
            get
            {
                var linesTaxCategories = this.Lines.Select(l => new { l.TaxCategory, l.Tax });
                var standard = linesTaxCategories.FirstOrDefault(c => c.TaxCategory == "S");
                Tax = standard != null ? standard.Tax : linesTaxCategories.FirstOrDefault().Tax;
                return standard != null ? "S" : linesTaxCategories.FirstOrDefault().TaxCategory;
            }
        }
    }
}
