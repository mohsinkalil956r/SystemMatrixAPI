using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ZATCA.API.Models.TaxData
{
    public class TaxDataVM
    {
        public double TotalWithoutTax { set; get; }
        public double TaxAmount { set; get; }
        [RegularExpression("E|S|Z", ErrorMessage = "The TaxCategory must be either 'E|Exempt' or 'S|Standard Rate'  or 'Z|Zero Rate' only.")]
        [Required(ErrorMessage = "TaxCategory data is Required")]

        public string TaxCategory { get; set; }
        public double Tax { get; set; }
        public int LineItemId { get; set; }

        public string TaxCategoryReasonCode { set; get; }
        public string TaxCategoryReason { set; get; }

    }
}