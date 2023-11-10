using System.ComponentModel.DataAnnotations;
using ZATCA.API.Models.Customer;
using ZATCA.API.Models.LinetItem;
using ZATCA.API.Models.Supplier;
using ZATCA.API.Models.TaxData;
using ZATCA.DAL.DB.Entities;

namespace ZATCA.API.Models.InvoiceData
{
    public class InvoiceDataVM
    {

  
        //Inv/2023/2/131231
        public string InvoiceNumber { get; set; }
        /// <summary>
        /// GUID / UUID
        /// </summary>
        public string Id { get; set; }
        [RegularExpression("1|2", ErrorMessage = "The InvoiceType must be either '1|Standard' or '2|Simplified' only.")]
        [Required(ErrorMessage = "InvoiceType data is Required")]

        public int InvoiceType { get; set; }
        [RegularExpression("388|383|381", ErrorMessage = "The InvoiceTypeCode must be either '388|Invoice' or 'Debit|383' 'Credit|381'only.")]
        [Required(ErrorMessage = "InvoiceTypeCode data is Required")]

        public int InvoiceTypeCode { get; set; }
        [RegularExpression("0200000|0100000", ErrorMessage = "The TransactionTypeCode must be either 'Simplified|0200000' or 'Standard|0100000' only.")]
        [Required(ErrorMessage = "TransactionTypeCode data is Required")]

        public string TransactionTypeCode { get; set; }

        public string Notes { get; set; }
        [Required(ErrorMessage = "Order is Required")]
        public int Order { get; set; }
        [Required(ErrorMessage = "IssueDate is Required")]
        public string IssueDate { get; set; }
        [Required(ErrorMessage = "IssueTime is Required")]
        public string IssueTime { get; set; }

        public string PreviousInvoiceHash { get; set; }
        [Required(ErrorMessage = "Lines data is Required")]
        public List<LineItemVM> Lines { get; set; }
       
        //public SupplierVM Supplier { get; set; }

        public double Discount { get; set; }

        public string ReferenceId { get; set; }
        [RegularExpression("10|30|42|48|1", ErrorMessage = "The PaymentMeansCode must be either '10|Cash' or'48|DebitCard' or'42|BankTransfer' or 30|Creditcard or '1|Check' only.")]
        [Required(ErrorMessage = "PaymentMeansCode data is Required")]
        public int PaymentMeansCode { get; set; }


        [Required(ErrorMessage = "Customer data is Required")]

        public CustomerVM Customer { get; set; }

        public string DeliveryDate { get; set; }
        [RegularExpression("15", ErrorMessage = "The Tax must be either '15' or '10' only.")]

        [Required(ErrorMessage = "Tax is Required")]
        public double Tax { get; set; } = 15;

        public int LinesCount
        {
            get { return this.Lines.Count; }
        }

        public List<TaxDataVM> SubTotals
        {
            get; set;
        }

        public string DiscountTaxCategory
        {
            get
           ; set;
        }
        /// <summary>
        /// VAT category taxable amount (BT-116) = ∑(Invoice line net amounts (BT-113)) − Document level allowance amount (BT-93)
        /// VAT category tax amount(BT-117) = VAT category taxable amount(BT-116) × (VAT rate (BT-119) ÷ 100)
        /// </summary>
        public double TaxAmount
        {
            get
            ; set;
        }

        public double TotalWithTax
        {
            get; set;
        }

        /// <summary>
        /// Invoice total amount without VAT (BT109) = Σ Invoice line net amount (BT-131) - Sum of allowances on document level (BT-107)
        /// Item line net amount (BT-131) = ((Item net price (BT-146) ÷ Item price base quantity(BT-149)) 
        ///     × (Invoiced Quantity (BT-129)) −Invoice line allowance amount(BT136)
        /// </summary>
        public double TotalWithoutTax
        {
            get; set;

        }

        public double TotalWithoutTaxAndDiscount
        {
            get; set;
        }


    }
}