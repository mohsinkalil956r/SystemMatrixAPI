using ZATCA.SDK.Helpers.Zatca.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ZATCA.SDK.Helpers.Zatca
{
    public interface IZatcaCSIDIssuer
    {
        Task<CSIDResultModel> OnboardingCSIDAsync(InputCSIDOnboardingModel model);

        Task<CSIDResultModel> RenewCSIDAsync(InputCSIDRenewingModel model);
    }
}
