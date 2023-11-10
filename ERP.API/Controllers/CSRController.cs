using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZATCA.API.Models.Company;
using ZATCA.API.Models;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;
using ZATCA.API.Models.CSR;
using System.Diagnostics;
using Microsoft.Win32;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;
using System.Net;
using ZATCA.API.Models.Users;
using System.Runtime.ConstrainedExecution;

namespace ZATCA.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CSRController : ControllerBase
    {
        private readonly ICSRRepository _repository;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private readonly ICompanyRepository _companyRepository;

        public CSRController(ICSRRepository repository, IWebHostEnvironment webHostEnvironment, ICompanyRepository companyRepository)
        {
            this._repository = repository;
            this._companyRepository = companyRepository;
            _webHostEnvironment = webHostEnvironment;
        }



        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var csr = this._repository.Get();

            var csrData = csr.Select(p => new CSRGetVM
            {
                Id = p.Id,
                VatNumber = p.VatNumber,
                TaxPayerName = p.TaxPayerName,
                TaxPayerEmail = p.TaxPayerEmail,
                InvoiceType = p.InvoiceType,
                Address = p.Address,
                BusinessCategory = p.BusinessCategory,
                CSRFor = p.CSRFor,
                CompanyId = p.CompanyId,

                CsrFile = p.CsrFile,
                Cnf = p.Cnf,
                Pem = p.Pem

            }).ToList();

            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
                data = csrData,
            });

        }








        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var csr = await this._repository.Get(id).FirstOrDefaultAsync();

            if (csr != null && !string.IsNullOrEmpty(csr.CsrFile) && !string.IsNullOrEmpty(csr.Cnf) && !string.IsNullOrEmpty(csr.Pem))
            {

                var apiResponse = new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                    data = new
                    {
                        csr.CsrFile,
                        csr.Cnf,
                        csr.Pem,

                    }
                };

                return Ok(apiResponse);
            }

            return NotFound();

        }

        //[Authorize(Roles = "Admin,Company")]
        [Route("getbycompany/{id}")]
        [HttpGet]
        public async Task<IActionResult> GetByCompany(int id)
        {
            var csr = await this._repository.Get().Where(x => x.CompanyId == id).FirstOrDefaultAsync();

            if (csr != null && !string.IsNullOrEmpty(csr.CsrFile) && !string.IsNullOrEmpty(csr.Cnf) && !string.IsNullOrEmpty(csr.Pem) && !string.IsNullOrEmpty(csr.InvoiceType) && !string.IsNullOrEmpty(csr.CSRFor))
            {

                var apiResponse = new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                    data = new
                    {
                        csr.InvoiceType,
                        csr.CSRFor,

                        csr.CsrFile,
                        csr.Cnf,
                        csr.Pem,

                    }
                };

                return Ok(apiResponse);
            }

            return NotFound();

        }




        // POST api/<ValuesController>
        //[Authorize(Roles = "Admin,Company")]
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CSRPostVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }


            return await GenerateCSR(model);

        }

        private async Task<IActionResult> GenerateDummyCSR(CSRPostVM csrData)
        {
            var folderPath = Path.Combine(this._webHostEnvironment.WebRootPath, "Data");
            var templatePath = Path.Combine(folderPath, "CSRconfigTemplate.cnf");
            var csrFilePath = Path.Combine(folderPath, "Csr", "CSRconfig.cnf");

            if (System.IO.File.Exists(csrFilePath))
            {
                var folderToCreateFilesIn = Path.Combine(this._webHostEnvironment.WebRootPath, "Data", "Csr", $"Company_{csrData.CompanyId}");

                if (!System.IO.Directory.Exists(folderToCreateFilesIn))
                {
                    System.IO.Directory.CreateDirectory(folderToCreateFilesIn);
                }

                var companyCSRfilepath = Path.Combine(folderToCreateFilesIn, "CSRconfig.cnf");

                ReplaceTemplateVariables(templatePath, companyCSRfilepath, csrData);

                var pkCmd = string.Format("openssl ecparam -name secp256k1 -genkey -noout -out PrivateKey.pem");
                ExecuteGitCommand(folderToCreateFilesIn, pkCmd);

                await System.IO.File.WriteAllTextAsync(Path.Combine(folderToCreateFilesIn, "CSR.csr"),
                    "-----BEGIN CERTIFICATE REQUEST-----\r\nMIIByzCCAXICAQAwTzELMAkGA1UEBhMCU0ExFzAVBgNVBAsMDmFtbWFuIEJyYW5j\r\naGNoMRMwEQYDVQQKDApoYXlhIHlhZyAzMRIwEAYDVQQDDAkxMjcuMC4wLjEwVjAQ\r\nBgcqhkjOPQIBBgUrgQQACgNCAATbirYn/yv/OsHhFlMPvFcRxI3ntuk1iwtilNYu\r\nV2+95knDAshb5OFsIYCHo/kL00KvxLs4+s+r1g8vqUgpok8XoIHDMIHABgkqhkiG\r\n9w0BCQ4xgbIwga8wJAYJKwYBBAGCNxQCBBcTFVRTVFpBVENBLUNvZGUtU2lnbmlu\r\nZzCBhgYDVR0RBH8wfaR7MHkxGzAZBgNVBAQMEjEtaGF5YXwyLTIzNHwzLTM1NDEf\r\nMB0GCgmSJomT8ixkAQEMDzMxMDE3NTM5NzQwMDAwMzENMAsGA1UEDAwEMTEwMDEQ\r\nMA4GA1UEGgwHWmF0Y2EgMzEYMBYGA1UEDwwPRm9vZCBCdXNzaW5lc3MzMAoGCCqG\r\nSM49BAMCA0cAMEQCICrrO7mK6Ve6MNb+JSIFDf+AF28jWfIa3Hw9aXGU9/JnAiAr\r\nJpUsHxgTk8kPe4PJsITbIaySyHvzfptqEMfDj7P7aw==\r\n-----END CERTIFICATE REQUEST-----"
                    );

                var csrFileContent = System.IO.File.ReadAllText(Path.Combine(folderToCreateFilesIn, "CSR.csr"));
                var cnfFileContent = System.IO.File.ReadAllText(companyCSRfilepath);
                var pemFileContent = System.IO.File.ReadAllText(Path.Combine(folderToCreateFilesIn, "PrivateKey.pem"));

                var csr = new CSR
                {
                    VatNumber = csrData.VatNumber,
                    TaxPayerName = csrData.TaxPayerName,
                    TaxPayerEmail = csrData.TaxPayerEmail,
                    InvoiceType = csrData.InvoiceType,
                    Address = csrData.Address,
                    BusinessCategory = csrData.BusinessCategory,
                    CSRFor = csrData.CSRFor,
                    CompanyId = csrData.CompanyId,

                    CsrFile = csrFileContent,
                    Cnf = cnfFileContent,
                    Pem = pemFileContent
                };

                _repository.Add(csr);
                await _repository.SaveChanges();

                return Ok();
            }

            return BadRequest("Template file not found.");
        }

        private async Task<IActionResult> GenerateCSR(CSRPostVM csrData)
        {
            var companyData = await this._companyRepository.Get().FirstOrDefaultAsync();

            var folderPath = Path.Combine(this._webHostEnvironment.WebRootPath, "Data");
            var templatePath = Path.Combine(folderPath, "CSRconfigTemplate.cnf");
            var csrFilePath = Path.Combine(folderPath, "Csr", "CSRconfig.cnf");

            if (System.IO.File.Exists(csrFilePath))
            {
                var folderToCreateFilesIn = Path.Combine(this._webHostEnvironment.WebRootPath, "Data", "Csr", $"Company_{csrData.CompanyId}");

                if (!System.IO.Directory.Exists(folderToCreateFilesIn))
                {
                    System.IO.Directory.CreateDirectory(folderToCreateFilesIn);
                }

                var companyCSRfilepath = Path.Combine(folderToCreateFilesIn, "CSRconfig.cnf");

                ReplaceTemplateVariables(templatePath, companyCSRfilepath, csrData);

                var pkCmd = string.Format("openssl ecparam -name secp256k1 -genkey -noout -out PrivateKey.pem");
                ExecuteGitCommand(folderToCreateFilesIn, pkCmd);
                var csrCmd = string.Format("openssl req -new -sha256 -key PrivateKey.pem -extensions v3_req -config CSRconfig.cnf -out CSR.csr");
                ExecuteGitCommand(folderToCreateFilesIn, csrCmd);

                var csrFileContent = System.IO.File.ReadAllText(Path.Combine(folderToCreateFilesIn, "CSR.csr"));
                var cnfFileContent = System.IO.File.ReadAllText(companyCSRfilepath);
                var pemFileContent = System.IO.File.ReadAllText(Path.Combine(folderToCreateFilesIn, "PrivateKey.pem"));


                //Replace Csr if same Company exists
                var existingCompany = _repository.Get().Where(p => p.CompanyId == csrData.CompanyId).FirstOrDefault();

                if (existingCompany != null)
                {
                    existingCompany.VatNumber = csrData.VatNumber;
                    existingCompany.TaxPayerName = csrData.TaxPayerName;
                    existingCompany.TaxPayerEmail = csrData.TaxPayerEmail;
                    existingCompany.InvoiceType = csrData.InvoiceType;
                    existingCompany.Address = csrData.Address;
                    existingCompany.BusinessCategory = csrData.BusinessCategory;
                    existingCompany.CSRFor = csrData.CSRFor;
                    existingCompany.CompanyId = csrData.CompanyId;

                    existingCompany.CsrFile = csrFileContent;
                    existingCompany.Cnf = cnfFileContent;
                    existingCompany.Pem = pemFileContent;

                    _repository.Update(existingCompany);
                }
                else
                {
                    var csr = new CSR
                    {
                        VatNumber = csrData.VatNumber,
                        TaxPayerName = csrData.TaxPayerName,
                        TaxPayerEmail = csrData.TaxPayerEmail,
                        InvoiceType = csrData.InvoiceType,
                        Address = csrData.Address,
                        BusinessCategory = csrData.BusinessCategory,
                        CSRFor = csrData.CSRFor,
                        CompanyId = csrData.CompanyId,

                        CsrFile = csrFileContent,
                        Cnf = cnfFileContent,
                        Pem = pemFileContent
                    };

                    _repository.Add(csr);
                }

                await _repository.SaveChanges();

                return Ok();
            }

            return BadRequest("Template file not found.");
        }


        private void ReplaceTemplateVariables(string templatePath, string csrFilePath, CSRPostVM csrData)
        {
            var templateContent = System.IO.File.ReadAllText(templatePath);

            templateContent = templateContent.Replace("@Vat", csrData.VatNumber.ToString().Substring(0, 15));
            templateContent = templateContent.Replace("@Name", csrData.TaxPayerName);
            //templateContent = templateContent.Replace("@Branch", csrData.VatNumber.ToString().Substring(0, 10));
            templateContent = templateContent.Replace("@Branch", "Riyadh Branch");
            templateContent = templateContent.Replace("@Title", csrData.InvoiceType);
            templateContent = templateContent.Replace("@Email", csrData.TaxPayerEmail);
            templateContent = templateContent.Replace("@Address", csrData.Address);
            templateContent = templateContent.Replace("@Category", csrData.BusinessCategory);
            templateContent = templateContent.Replace("@Guid", Guid.NewGuid().ToString());
            templateContent = templateContent.Replace("@Pre", "PRE");

            System.IO.File.WriteAllText(csrFilePath, templateContent);
        }


        private static void ExecuteGitCommand(string folderPath, string cmd)
        {
            if (System.IO.Directory.Exists(folderPath) == false)
            {
                System.IO.Directory.CreateDirectory(folderPath);
            }

            //var programFilePath = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
            var programFilePath = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry64)
            .OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion")?.GetValue("ProgramFilesDir") as string;
            var gitShellPath = Path.Combine(programFilePath, "Git\\bin\\sh.exe");
            ProcessStartInfo procStartInfo = new ProcessStartInfo(gitShellPath, "-c \" " + cmd);
            procStartInfo.CreateNoWindow = false;
            procStartInfo.WorkingDirectory = folderPath;
            Process proc = new Process();
            proc.StartInfo = procStartInfo;
            proc.Start();
            proc.WaitForExit();
            proc.Close();
        }




    }
}
