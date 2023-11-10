using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.SDK.Helpers.Zatca.Models
{
    public class InputCSIDOnboardingModel
    {
        public string csr { get; set; }
        public string otp { get; set; }
        public Supplier Supplier { get; set; }
    }
    public class InputCSIDRenewingModel
    {
        public string csr { get; set; }
        public string otp { get; set; }
    }
    public class CSIDResultModel
    {
        public string ErrorMessage { get; set; }
        /// <summary>
        /// Working as User Name (in Authentication)
        /// </summary>
        public string Certificate { get; set; }
        public string Secret { get; set; }

        public DateTime ExpiredDate { get; set; }
        public DateTime StartedDate { get; set; }
    }
}
