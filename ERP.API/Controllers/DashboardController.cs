using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ZATCA.API.Models.CSR;
using ZATCA.API.Models;
using ZATCA.DAL.Repositories.Abstraction;
using Microsoft.EntityFrameworkCore;
using ZATCA.API.Models.InvoiceRequest;
using ZATCA.API.Models.ZatcaResponse;
using System.Linq;
using System.Globalization;

namespace ZATCA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IOnBoardingCSIDRepository _onBoardingCSIDRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IInvoiceRequestRepository _invoiceRequestRepository;


        public DashboardController(IUserRepository userRepository, IOnBoardingCSIDRepository onBoardingCSIDRepository, ICompanyRepository companyRepository,
            IInvoiceRequestRepository invoiceRequestRepository)
        {
            _userRepository = userRepository;
            _onBoardingCSIDRepository = onBoardingCSIDRepository;
            _companyRepository = companyRepository;
            _invoiceRequestRepository = invoiceRequestRepository;
        }


        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var startDate = DateTime.Now.AddDays(-6).Date;
            var endDate = DateTime.Now.AddDays(1).Date;

            var users = _userRepository.Get();
            var onBoardedDevices = _onBoardingCSIDRepository.Get();
            var companies = _companyRepository.Get();

            var requestDate = _invoiceRequestRepository.Get().Select(r => r.RequestDate);

            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");


            var userId = companyIdClaim.Value;
            // userId contains the user's ID as string


            var query = this._invoiceRequestRepository.Get().AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.CreatedBy.ToString() == userId);
                // The user has the "Admin" role

            }

            //Dashboard Counts
            var invoicesWithWarnings = query
                .Where(x => x.ZatcaResponses.Any(z => !string.IsNullOrEmpty(z.WarnngMessage) && z.WarnngMessage != "[]"))
                .ToList();

            var invoicesWithoutWarnings = query
                .Where(x => x.ZatcaResponses.Any(z => string.IsNullOrEmpty(z.WarnngMessage) && z.WarnngMessage != "[]"))
                .ToList();

            var invoicesWithErrors = query
                .Where(x => x.ZatcaResponses.Any(z => !string.IsNullOrEmpty(z.ErrorMessage) && z.ErrorMessage != "[]"))
                .ToList();



            //Invoices from DB For Weekly chart
            var invoicesWithWarningsForWeeklyChart = query
                .Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate
                        && x.ZatcaResponses.Any(z => !string.IsNullOrEmpty(z.WarnngMessage) && z.WarnngMessage != "[]"))
                .ToList();

            var invoicesWithoutWarningsForWeeklyChart = query
                .Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate
                        && x.ZatcaResponses.Any(z => string.IsNullOrEmpty(z.WarnngMessage) && z.WarnngMessage != "[]"))
                .ToList();

            var invoicesWithErrorsForWeeklyChart = query
                .Where(x => x.RequestDate >= startDate && x.RequestDate <= endDate
                        && x.ZatcaResponses.Any(z => !string.IsNullOrEmpty(z.ErrorMessage) && z.ErrorMessage != "[]"))
                .ToList();

            //Weekly Chart Data
            var invoicesWithWarningsByDay = invoicesWithWarningsForWeeklyChart
                .GroupBy(x => x.RequestDate.Date)
                .Select(group => new
                {
                    day = group.Key.DayOfWeek.ToString(),
                    total = group.Count()
                })
                .ToList();

            var invoicesWithoutWarningsByDay = invoicesWithoutWarningsForWeeklyChart
                .GroupBy(x => x.RequestDate.Date)
                .Select(group => new
                {
                    day = group.Key.DayOfWeek.ToString(),
                    total = group.Count()
                })
                .ToList();

            var invoicesWithErrorsByDay = invoicesWithErrorsForWeeklyChart
                .GroupBy(x => x.RequestDate.Date)
                .Select(group => new
                {
                    day = group.Key.DayOfWeek.ToString(),
                    total = group.Count()
                })
                .ToList();



            //Monthly Chart Data
            var invoicesWithWarningsByMonth = invoicesWithWarnings
                .GroupBy(x => new { x.RequestDate.Year, x.RequestDate.Month })
                .Select(group => new
                {
                    month = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM", CultureInfo.InvariantCulture),
                    total = group.Count()
                })
                .ToList();

            var invoicesWithoutWarningsByMonth = invoicesWithoutWarnings
                .GroupBy(x => new { x.RequestDate.Year, x.RequestDate.Month })
                .Select(group => new
                {
                    month = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM", CultureInfo.InvariantCulture),
                    total = group.Count()
                })
                .ToList();

            var invoicesWithErrorsByMonth = invoicesWithErrors
                .GroupBy(x => new { x.RequestDate.Year, x.RequestDate.Month })
                .Select(group => new
                {
                    month = new DateTime(group.Key.Year, group.Key.Month, 1).ToString("MMMM", CultureInfo.InvariantCulture),
                    total = group.Count()
                })
                .ToList();





            var userCount = users.Count();
            var deviceCount = onBoardedDevices.Count();
            var companyCount = companies.Count();

            return Ok(new APIResponse<object>
            {
                IsError = false,
                data = new
                {
                    userCount,
                    deviceCount,
                    companyCount,

                    WithWarnings = invoicesWithWarnings.Count(),
                    WithoutWarnings = invoicesWithoutWarnings.Count(),
                    WithErrors = invoicesWithErrors.Count(),

                    invoicesWithWarningsByDay,
                    invoicesWithoutWarningsByDay,
                    invoicesWithErrorsByDay,

                    invoicesWithWarningsByMonth,
                    invoicesWithoutWarningsByMonth,
                    invoicesWithErrorsByMonth
                }
            });
        }





    }
}
