using ZATCA.SDK.Helpers.Zatca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.SDK.Helpers.Zatca
{
    public interface IZatcaCsrReader
    {
        CsrInvoiceTypeRestriction GetCsrInvoiceType(string csr);
    }
}
