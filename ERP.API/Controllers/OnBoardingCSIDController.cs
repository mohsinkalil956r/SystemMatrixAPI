using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Net.WebSockets;
using System.Runtime.ConstrainedExecution;
using System.Text;
using ZATCA.API.Models;
using ZATCA.API.Models.CSR;
using ZATCA.API.Models.OnBoardingCSID;
using ZATCA.DAL;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;
using ZATCA.SDK.Helpers.Zatca;
using ZATCA.SDK.Helpers.Zatca;
using ZATCA.SDK.Helpers.Zatca.Helpers;
//using ZATCA.SDK.Zatca;
using ZATCA.SDK.Helpers.Zatca.Models;
using ZXing;
using Supplier = ZATCA.SDK.Helpers.Zatca.Models.Supplier;

namespace ZATCA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class OnBoardingCSIDController : ControllerBase
    {
        private readonly IWebHostEnvironment _webHostEnvironment;

        private readonly IOnBoardingCSIDRepository _repository;
        private readonly IZatcaCSIDIssuer _zatcaCSIDIssuer;
        private readonly ICSRRepository _cSRRepository;
        private readonly ICompanyRepository _companyRepository;
        public OnBoardingCSIDController(IOnBoardingCSIDRepository repository, IZatcaCSIDIssuer zatcaCSIDIssuer
            , ICSRRepository cSRRepository, IWebHostEnvironment webHostEnvironment, ICompanyRepository companyRepository)
        {
            _zatcaCSIDIssuer = zatcaCSIDIssuer;
            _cSRRepository = cSRRepository;
            this._repository = repository;
            this._companyRepository = companyRepository;

            _webHostEnvironment = webHostEnvironment;
        }




        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var csid = this._repository.Get();

            var csidData = csid.Select(p => new OnBoardingCSIDGetVM
            {
                Id = p.Id,
                CompanyId = p.CompanyId,
                UserName = p.UserName,
                Secret = p.Secret,
                StartedDate = p.StartedDate,
                ExpiredDate = p.ExpiredDate,

            }).ToList();

            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
                data = csidData,
            });

        }




        //[Authorize(Roles = "Admin,Company")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] OnBoardingCSIDPostVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            return await OnBoardingCSID(model);
        }

        private async Task<IActionResult> OnBoardingCSID(OnBoardingCSIDPostVM csidData)
        {

            var companyData = await this._companyRepository.Get(csidData.CompanyId).FirstOrDefaultAsync();
            var csid = await this._cSRRepository.Get().FirstOrDefaultAsync();

            var csrFile = "CSR.csr";
            var relativePath = Path.Combine("Data", "Csr", $"Company_{csidData.CompanyId}", csrFile);
            var webRootPath = this._webHostEnvironment.WebRootPath;
            var csidPath = Path.Combine(webRootPath, relativePath);
            var csidCsr = System.IO.File.ReadAllText(csidPath);


            var bytes = System.Text.Encoding.UTF8.GetBytes(csidCsr.Replace("\r", ""));
            var encode = Convert.ToBase64String(bytes);

            if (csidCsr == null)
            {
                return BadRequest(ModelState);
            }
            else
            {
                if (!string.IsNullOrEmpty(csid.CsrFile))
                {
                    var _csrReader = new ZatcaCsrReader(this._companyRepository);
                    var result = await _zatcaCSIDIssuer.OnboardingCSIDAsync(new InputCSIDOnboardingModel
                    {

                        csr = (encode),
                        otp = csidData.OTP,
                        Supplier = new Supplier
                        {
                            SellerName = companyData.Name,
                            SellerTRN = companyData.VatNumber,
                            AdditionalStreetAddress = companyData.AdditionalStreetAddress,
                            BuildingNumber = companyData.BuildingNumber,
                            CityName = companyData.CityName,
                            IdentityNumber = companyData.IdentityNumber,
                            IdentityType = companyData.IdentityType,
                            CountryCode = companyData.CountryCode,
                            DistrictName = companyData.DistrictName,
                            PostalCode = companyData.PostalCode,
                            StreetName = companyData.StreetName,
                        },
                    });


                    if (result.ErrorMessage == null)
                    {

                        var existingCompany = _repository.Get().Where(p => p.CompanyId == csidData.CompanyId).FirstOrDefault();

                        if (existingCompany != null)
                        {
                            existingCompany.UserName = result.Certificate;
                            existingCompany.Secret = result.Secret;
                            existingCompany.StartedDate = result.StartedDate;
                            existingCompany.ExpiredDate = result.ExpiredDate;
                            _repository.Update(existingCompany);
                        }

                        else
                        {
                            _repository.Add(new DAL.DB.Entities.OnBoardingCSID
                            {
                                CompanyId = csidData.CompanyId,
                                UserName = result.Certificate,
                                Secret = result.Secret,
                                StartedDate = result.StartedDate,
                                ExpiredDate = result.ExpiredDate,
                            });

                        };

                        await _repository.SaveChanges();


                        return Ok(new APIResponse<object>
                        {
                            data = result

                        });

                    }
                    else
                    {

                        return Ok(new APIResponse<object>
                        {
                            IsError = true,
                            Message = result.ErrorMessage
                        }) ;
                    }


                }
                else { return NotFound(new APIResponse <object>{ data= null,Message= "Csr Does Not Exist"}); }
            }
            return Ok(new APIResponse<object>
            {
                Message = "this"
            });
        }
        




    }
}
