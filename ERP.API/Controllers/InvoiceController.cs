using Microsoft.AspNetCore.Mvc;


using ZATCA.API.Models.InvoiceData;
using ZATCA.DAL.Repositories.Abstraction;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ZXing;
using ZXing.QrCode;
using ZXing.QrCode;

using System.Drawing; // Add this namespace for working with images

using System.Drawing.Imaging;
using ZATCA.API.Models;
using ZATCA.SDK.Helpers.Zatca.Interfaces;
using ZATCA.SDK.Helpers.Zatca.Models;
using ZATCA.SDK.Helpers.Zatca;
using ZXing.Common;
using ZATCA.DAL.DB.Entities;
using LineItem = ZATCA.SDK.Helpers.Zatca.Models.LineItem;
using Supplier = ZATCA.SDK.Helpers.Zatca.Models.Supplier;
using Customer = ZATCA.SDK.Helpers.Zatca.Models.Customer;
using ZATCA.API.Models.LinetItem;
using Newtonsoft.Json;
using ZATCA.API.Models.Company;
using Microsoft.EntityFrameworkCore;
using ZATCA.API.Models.InvoiceRequest;
using ZATCA.API.Models.CompanyVM;
using ZATCA.API.Models.ZatcaResponse;
using Microsoft.AspNetCore.Identity;
using System.Diagnostics.Eventing.Reader;
using ZATCA.API.Models.Users;
using Lucene.Net.Messages;
using ZATCA.API.Models.ZatcaResponse;
using ZATCA.API.Models.ValidationResult;
using ZATCA.API.Models.ErrorMessage;
using ZATCA.API.Models.WarnngMessage;
using Microsoft.AspNetCore.Authorization;
using System.ComponentModel.Design;
using Lucene.Net.Search;
using System.Security.Claims;

namespace ZATCA.API.Controllers
{
    //[Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InvoiceController : ControllerBase
    {
        private readonly IInvoiceRequestRepository _invoiceRequestRepository;
        private readonly IInvoiceInfoGenerator _invoiceInfoGenerator;
        private readonly IOnBoardingCSIDRepository _onBoardingrepository;
        private readonly IZatcaReporter _reporter;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly UserManager<SystemUser> _userManager;
        private readonly ICompanyRepository _companyRepository;

        public InvoiceController(IZatcaReporter reporter, UserManager<SystemUser> userManager, ICompanyRepository companyRepository, IInvoiceRequestRepository invoiceRequestRepository, IInvoiceInfoGenerator invoiceInfoGenerator,
       IOnBoardingCSIDRepository onBoardingrepository, IWebHostEnvironment webHostEnvironment)
        {
            this._userManager = userManager;
             _reporter= reporter;
            _onBoardingrepository = onBoardingrepository;
            //_companyRepository = companyRepository;
            _invoiceInfoGenerator = invoiceInfoGenerator;
            _invoiceRequestRepository = invoiceRequestRepository;
            _webHostEnvironment = webHostEnvironment;
            _companyRepository=companyRepository;

        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Get(string? searchQuery = "", int pageNumber = 1, int pageSize = 10, string searchField = "", DateTime? startDate =null, DateTime? endDate = null)
        {
            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");

            
                var userId = companyIdClaim.Value;
            // Now you can use the user's ID
            // userId contains the user's ID as a string

            
            var query = this._invoiceRequestRepository.Get().AsQueryable();
            if (!User.IsInRole("Admin"))
            {
                query = query.Where(p => p.CreatedBy.ToString() == userId);
                // The user has the "Admin" role
                // You can perform actions for users with this role
            }
          
            if (!startDate.HasValue)
            {
                startDate = DateTime.Now.AddYears(-1);
            }
           
            if (!endDate.HasValue)
            {
                endDate = DateTime.Now.AddYears(1);
            }
            // Apply search filter if searchQuery is provided and not null or empty
            if (!string.IsNullOrEmpty(searchQuery))
            {
           
                if (!string.IsNullOrEmpty(searchField))
                {
                    switch (searchField.ToLower())
                    {
                        case "companysname":
                            query = query.Where(p => p.Detail.Contains(searchQuery));
                            break;
                        case "requestid":
                            query = query.Where(p => p.Id.ToString()==(searchQuery));
                            break;
                        //case "status":
                        //    query = query.Where(p => p.RequestDate.ToString().Contains(searchQuery));
                        //    break;
                        // Add more cases for other search fields as needed
                        default:
                            query = query.Where(p => p.Detail.Contains(searchQuery));
                            break;
                    }
                }
                else {
                    query = query.Where(p => p.Detail.Contains(searchQuery));
                }
            }

            // Apply date range filter
            if (startDate != null || endDate != null)
            {
                query = query.Where(p => p.RequestDate >= startDate && p.RequestDate <= endDate);
            }

            // Get the total count of items without pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var invoices = await query.Include(x => x.ZatcaResponses).ToListAsync();

            var result = invoices.Select(p => new InvoiceRequestVM
            {
                Id = p.Id,
                Detail = p.Detail,
                Status = p.Status,
                CreatedBy = p.CreatedBy,
                RequestDate = p.RequestDate,
                Responses = p.ZatcaResponses.Select(x => new ZatcaResponseVM
                {
                    status= x.status,
                    ErrorMessage = x.ErrorMessage,
                    ResponseXML = x.ResponseXML,
                    InfoMessage = x.InfoMessage,
                    WarnngMessage = x.WarnngMessage,
                    InvoiceBase64 = x.InvoiceBase64,
                    Isreported = x.Isreported,
                    QrCode = x.QrCode,

                }).ToList(),
            }).ToList();

            var paginationResult = new PaginatedResult<InvoiceRequestVM>(result, totalCount);
            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
                data = paginationResult
            });
        }

        // GET api/<ValuesController>/5
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var invoiceRequest = await this._invoiceRequestRepository.Get(id).Include(res => res.ZatcaResponses).FirstOrDefaultAsync();
            if (invoiceRequest != null)
            {
                var data = new InvoiceRequestVM
                {
                    Id = invoiceRequest.Id,

                    Detail = invoiceRequest.Detail,
                    CreatedBy = invoiceRequest.CreatedBy,
                    RequestDate = invoiceRequest.RequestDate,
                    Status = invoiceRequest.Status,
                    
                    Responses = invoiceRequest.ZatcaResponses.Select(x => new ZatcaResponseVM
                    {
                        Isreported = x.Isreported,
                        ResponseXML = x.ResponseXML,
                        status = x.status,
                        InvoiceBase64 = x.InvoiceBase64,
                        InvoiceHash = x.InvoiceHash,
                        QrCode = x.QrCode,
                        SubmissionDate = x.SubmissionDate,
                        ErrorMessage = x.ErrorMessage,
                        WarnngMessage = x.WarnngMessage,
                        InfoMessage = x.InfoMessage,
                       
                    }).ToList(),

                };

                return Ok(new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                    data = data//.Responses.Select(x => x.ResponseXML).FirstOrDefault(),
                }) ;
            }

            return NotFound();

        }



        // POST api/<ValuesController>
       [Authorize]
        [HttpPost]

        public async Task<IActionResult> Post([FromBody] InvoiceDataVM model, int CompanyId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var csidData = _onBoardingrepository.Get().Where(p => p.CompanyId == CompanyId).FirstOrDefault();
            var companyData = await this._companyRepository.Get(CompanyId).FirstOrDefaultAsync();
          
            
                var invoiceModel = new InvoiceDataModel
            {
                InvoiceNumber = model.InvoiceNumber,
                InvoiceTypeCode = model.InvoiceTypeCode,
                Id = Guid.NewGuid().ToString(),
                Order = model.Order,
                InvoiceType= model.InvoiceType,
                TransactionTypeCode = model.TransactionTypeCode,
                Tax = model.Tax,
                Lines = model.Lines.Select(x => new LineItem
                {
                    LineDiscount = x.LineDiscount,
                    Index = x.Index,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    NetPrice = x.NetPrice,
                    Tax = x.Tax,
                    TaxCategory = x.TaxCategory,
                    TaxCategoryReason = x.TaxCategoryReason,
                    TaxCategoryReasonCode = x.TaxCategoryReasonCode,
                }).ToList(),

                //Discount = 50,
                PaymentMeansCode = model.PaymentMeansCode,
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
                
                Customer = new Customer
                {
                    CustomerName = model.Customer.CustomerName,
                    IdentityNumber = model.Customer.IdentityNumber,
                    IdentityType = model.Customer.IdentityType,
                    VatRegNumber = model.Customer.VatRegNumber,
                    StreetName = model.Customer.StreetName,
                    BuildingNumber = model.Customer.BuildingNumber,
                    ZipCode = model.Customer.ZipCode,
                    CityName = model.Customer.CityName,
                    DistrictName = model.Customer.DistrictName,
                    RegionName = model.Customer.RegionName,
                },
                Discount=model.Discount,
                IssueDate = model.IssueDate,
                IssueTime = model.IssueTime,
                PreviousInvoiceHash = GetPreviousInvoiceHash(),
                //Notes = "Cancellation or suspension of the supplies after its occurrence either wholly or partially",
                //ReferenceId = "INV/2022/9/26/1"
            };


            var companyIdClaim = User.Claims.FirstOrDefault(c => c.Type == "CompanyId");
            var companyId = companyIdClaim.Value;
            var result = _invoiceInfoGenerator.GenerateXmlBeforeSigning(invoiceModel);
            if(csidData != null)
            {
                SharedData.UserName = csidData.UserName;
                SharedData.Secret = csidData.Secret;
            };
            var zatcaResponse= _reporter.ReportInvoiceAsync(invoiceModel);

          // invoiceData.Status = result.Success ? 1 : 0;
            var invoiceData = new InvoiceRequest
            {
                Detail = JsonConvert.SerializeObject(invoiceModel),
                RequestDate = DateTime.Now,
                CreatedBy = int.Parse(companyId),
                Status = 0,             
            };

            this._invoiceRequestRepository.Add(invoiceData);

            await this._invoiceRequestRepository.SaveChanges();
            if (zatcaResponse.Result.Success)
            {
                invoiceData.ZatcaResponses.Add(new ZatcaResponse
                {
                    ResponseXML= zatcaResponse.Result.Data.SignedXml,
                    Isreported= true,

                    status = zatcaResponse.Result.Data.ReportingStatus,
                    InvoiceBase64 = zatcaResponse.Result.Data.InvoiceBase64,
                    InvoiceHash = zatcaResponse.Result.Data.InvoiceHash,
                    QrCode = zatcaResponse.Result.Data.QrCode,
                    SubmissionDate = zatcaResponse.Result.Data.SubmissionDate,

                    ErrorMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.ErrorMessages),
                    WarnngMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.WarningMessages),
                    InfoMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.ReportingResult),
                     


                });
               
            }
            else
            {
                invoiceData.ZatcaResponses.Add(new ZatcaResponse
                {
                  ResponseXML = zatcaResponse.Result.Data.SignedXml,
                    Isreported = true,
                    status = zatcaResponse.Result.Data.ReportingStatus,
                    InvoiceBase64 = zatcaResponse.Result.Data.InvoiceBase64,
                    InvoiceHash = zatcaResponse.Result.Data.InvoiceHash,
                    QrCode = zatcaResponse.Result.Data.QrCode ?? "Empty",
                    SubmissionDate = zatcaResponse.Result.Data.SubmissionDate,
                    ErrorMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.ErrorMessages),
                    WarnngMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.WarningMessages),
                    InfoMessage = JsonConvert.SerializeObject(zatcaResponse.Result.Data.ReportingResult),
                    
                });

              //  this._invoiceRequestRepository.Add(invoiceData);

            }
            

             await this._invoiceRequestRepository.SaveChanges();
           var invoiceRequest = _invoiceRequestRepository.Get().OrderByDescending(x => x.Id).FirstOrDefault();

            return Ok(new APIResponse<object>
            {
                RequestId = invoiceRequest.Id.ToString(),
                Message ="",
                data = result
            }) ;

        }
        private string GetPreviousInvoiceHash()
        {
            // Select Top (1) * from invoices order by SubmissionDate
            // _context.invoices.OrderByDescending(i=> i.SubmissionDate).FirstOrDefault();
            return "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
        }

        private string GetNextInvoiceNumber()
        {
            return "INV/2022/9/26/2";
        }
        [HttpPost("QRDownload")]
         public async Task<IActionResult> DownloadQrCodeAsync([FromBody] InvoiceDataVM model,int CompanyId)
        {

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var companyData = await this._companyRepository.Get(CompanyId).FirstOrDefaultAsync();
            var invoiceModel = new InvoiceDataModel
            {
                InvoiceNumber = model.InvoiceNumber,
                InvoiceTypeCode = model.InvoiceTypeCode,
                Id = Guid.NewGuid().ToString(),
                Order = model.Order,
                TransactionTypeCode = model.TransactionTypeCode,
                Tax = model.Tax,
                Lines = model.Lines.Select(x => new LineItem
                {
                    LineDiscount = x.LineDiscount,
                    Index = x.Index,
                    ProductName = x.ProductName,
                    Quantity = x.Quantity,
                    NetPrice = x.NetPrice,
                    Tax = x.Tax,
                    TaxCategory = x.TaxCategory,
                    TaxCategoryReason = x.TaxCategoryReason,
                    TaxCategoryReasonCode = x.TaxCategoryReasonCode,
                }).ToList(),

                //Discount = 50,
                PaymentMeansCode = model.PaymentMeansCode,
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
               
                Customer = new Customer
                {
                    CustomerName = model.Customer.CustomerName,
                    IdentityNumber = model.Customer.IdentityNumber,
                    IdentityType = model.Customer.IdentityType,
                    VatRegNumber = model.Customer.VatRegNumber,
                    StreetName = model.Customer.StreetName,
                    BuildingNumber = model.Customer.BuildingNumber,
                    ZipCode = model.Customer.ZipCode,
                    CityName = model.Customer.CityName,
                    DistrictName = model.Customer.DistrictName,
                    RegionName = model.Customer.RegionName,
                },
                IssueDate = model.IssueDate,
                IssueTime = model.IssueTime,
                PreviousInvoiceHash = GetPreviousInvoiceHash(),
                //Notes = "Cancellation or suspension of the supplies after its occurrence either wholly or partially",
                //ReferenceId = "INV/2022/9/26/1"
            };
            var qrCode= _invoiceInfoGenerator.GenerateQrCode(invoiceModel);

            var qrCodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
                    Width = 300,  // Set the desired width of the QR code image
                    Height = 300  // Set the desired height of the QR code image
                }
            };

            var qrCodeBitmap = qrCodeWriter.Write(qrCode.ResultValue);

            // Specify the path where you want to save the QR code image

            var folderPath = Path.Combine(this._webHostEnvironment.WebRootPath, "QR");
            // Generate a unique filename, for example, using a timestamp
            var uniqueFileName = $"{model.InvoiceNumber}{DateTime.Now.Ticks}.png";
            var filePath = Path.Combine(folderPath, "qrCode.png");

            //  var imagePath = @"C:\Users\qrcode.png";

            // Save the QR code bitmap to the specified path
            var memStream = new MemoryStream();
            qrCodeBitmap.Save(memStream, ImageFormat.Png);
            //
            return new FileStreamResult(new MemoryStream(memStream.ToArray(), true), "image/png")
            {
                FileDownloadName = uniqueFileName
            };

            memStream.Dispose();
            // Return a response indicating the image path or other relevant information
            //  return Ok(new APIResponse<Object> { Message = filePath, data = qrCode });
        }

        [HttpGet("QR/{id}")]
        public IActionResult PostQR( int id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

          
            var invoicedata= _invoiceRequestRepository.Get(id).Include(w=>w.ZatcaResponses).FirstOrDefault();
           var qrCode= invoicedata.ZatcaResponses.Select(x=>x.QrCode).FirstOrDefault();
           // var qrCodeContent = _invoiceInfoGenerator.GenerateQrCode(model);

            if (qrCode == null || string.IsNullOrEmpty(qrCode.ToString()))
            {
                return BadRequest("Failed to generate QR code content.");
            }

            var qrCodeWriter = new BarcodeWriter
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions
                {
                    ErrorCorrection = ZXing.QrCode.Internal.ErrorCorrectionLevel.M,
                    Width = 300,  // Set the desired width of the QR code image
                    Height = 300  // Set the desired height of the QR code image
                }
            };

            var qrCodeBitmap = qrCodeWriter.Write(qrCode.ToString());

            // Specify the path where you want to save the QR code image

            var folderPath = Path.Combine(this._webHostEnvironment.WebRootPath, "QR");
            // Generate a unique filename, for example, using a timestamp
            var uniqueFileName = $"{DateTime.Now.Ticks}.png";
            var filePath = Path.Combine(folderPath, "qrCode.png");

            //  var imagePath = @"C:\Users\qrcode.png";

            // Save the QR code bitmap to the specified path
            var memStream = new MemoryStream();
            qrCodeBitmap.Save(memStream, ImageFormat.Png);
            //
            return new FileStreamResult(new MemoryStream(memStream.ToArray(), true), "image/png")
            {
                FileDownloadName = uniqueFileName
            };

         
            memStream.Dispose();
            // Return a response indicating the image path or other relevant information
          //  return Ok(new APIResponse<Object> { Message = filePath, data = qrCode });
         }
    }

}

