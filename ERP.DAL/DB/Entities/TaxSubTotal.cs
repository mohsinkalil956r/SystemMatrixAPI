using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.DAL.DB.Entities
{
    public class TaxSubTotal
    {
        public int Id { get; set; }
        public double TotalWithoutTax { set; get; }
        public double TaxAmount { set; get; }

        public string TaxCategory { get; set; }
        public double Tax { get; set; }
        public int LineItemId { get; set; }
        [NotMapped]
        public LineItem LineItem { get; set; }

        //public string TaxCategoryReasonCode
        //{
        //    get
        //    {
        //        switch (TaxCategory)
        //        {
        //            case "E":
        //                return "VATEX-SA-29"; // Financial services
        //            case "Z":
        //                return "VATEX-SA-36"; // Qualifying metals
        //            default:
        //                return null;
        //        }
        //    }
        //}
        public string TaxCategoryReasonCode { set; get; }
        public string TaxCategoryReason { set; get; }
        //public string TaxCategoryReason
        //{
        //    get
        //    {
        //        switch (TaxCategory)
        //        {
        //            case "E":
        //                return ZatcaCodeLists.VatCategories.FirstOrDefault(c => c.Value == "E").Name;
        //            case "Z":
        //                return "Qualifying metals";//ZatcaCodeLists.VatCategories.FirstOrDefault(c => c.Value == "Z").Name;
        //            default:
        //                return null;
        //        }
        //    }
        //}
    }
}
