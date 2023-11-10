using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ZATCA.API.Models;
using ZATCA.API.Models.Company;
using ZATCA.API.Models.CompanyVM;
using ZATCA.DAL.DB.Entities;
using ZATCA.DAL.Repositories.Abstraction;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ERP.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyRepository _repository;
        public CompanyController(ICompanyRepository repository)
        {
            this._repository = repository;
        }

        [HttpGet]
        public async Task<IActionResult> Get(string? searchQuery = "", int pageNumber = 1, int pageSize = 10)
        {
            var query = this._repository.Get().AsQueryable();

            // Apply search filter if searchQuery is provided and not null or empty
            if (!string.IsNullOrEmpty(searchQuery))
            {
                query = query.Where(p =>
                    p.Name.Contains(searchQuery) ||
                    p.Email.Contains(searchQuery) ||
                    p.BusinessCategory.Contains(searchQuery) ||
                    p.Number.Contains(searchQuery) ||
                    p.Token.Contains(searchQuery) ||
                    p.CountryCode.ToString().Contains(searchQuery) ||
                    p.VatNumber.ToString().Contains(searchQuery) ||
                    p.Industry.Contains(searchQuery) ||
                    p.UnitName.Contains(searchQuery) ||
                    p.ArchiveType.Contains(searchQuery) ||
                    p.Environment.Contains(searchQuery) ||
                    p.ArchiveConfig.Contains(searchQuery) ||
                    p.ContractStart.ToString().Contains(searchQuery) ||
                    p.ContractEnd.ToString().Contains(searchQuery) ||
                    p.Address.Contains(searchQuery) ||
                    p.CompanyIsActive.ToString().Contains(searchQuery) ||
                    p.AdditionalStreetAddress.Contains(searchQuery) ||
                    p.BuildingNumber.Contains(searchQuery) ||
                    p.CityName.Contains(searchQuery) ||
                    p.IdentityNumber.Contains(searchQuery) ||
                    p.IdentityType.Contains(searchQuery) ||
                    p.DistrictName.Contains(searchQuery) ||
                    p.PostalCode.Contains(searchQuery) ||
                    p.StreetName.Contains(searchQuery)
                    );
            }

            // Get the total count of items without pagination
            var totalCount = await query.CountAsync();

            // Apply pagination
            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);

            var Company = await query.ToListAsync();

            var result = Company.Select(p => new CompanyGetResponseVM
            {
                Id = p.Id,
                Name = p.Name,
                Email = p.Email,
                BusinessCategory = p.BusinessCategory,
                Number = p.Number,
                Token = p.Token,
                CountryCode = p.CountryCode,
                VatNumber = p.VatNumber,
                Industry = p.Industry,
                UnitName = p.UnitName,
                ArchiveType = p.ArchiveType,
                Environment = p.Environment,
                ArchiveConfig = p.ArchiveConfig,
                ContractStart = p.ContractStart,
                ContractEnd = p.ContractEnd,
                Address = p.Address,
                CompanyIsActive = p.CompanyIsActive,

                AdditionalStreetAddress = p.AdditionalStreetAddress,
                BuildingNumber = p.BuildingNumber,
                CityName = p.CityName,
                IdentityNumber = p.IdentityNumber,
                IdentityType = p.IdentityType,
                DistrictName = p.DistrictName,
                PostalCode = p.PostalCode,
                StreetName = p.StreetName

            }).ToList();

            var paginationResult = new PaginatedResult<CompanyGetResponseVM>(result, totalCount);
            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
                data = paginationResult
            });
        }

        // GET api/<ValuesController>/5
        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var company = await this._repository.Get(id).FirstOrDefaultAsync();
            if (company != null)
            {
                var apiResponse = new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                    data = new
                    {
                        company.Id,
                        company.Name,
                        company.Email,
                        company.BusinessCategory,
                        company.Number,
                        company.Token,
                        company.CountryCode,
                        company.VatNumber,
                        company.Industry,
                        company.UnitName,
                        company.ArchiveType,
                        company.Environment,
                        company.ArchiveConfig,
                        company.ContractStart,
                        company.ContractEnd,
                        company.Address,
                        company.CompanyIsActive,

                        company.AdditionalStreetAddress,
                        company.BuildingNumber,
                        company.CityName,
                        company.IdentityNumber,
                        company.IdentityType,
                        company.DistrictName,
                        company.PostalCode,
                        company.StreetName
                    }
                };

                return Ok(apiResponse);
            }

            return NotFound();

        }

        // POST api/<ValuesController>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] CompanyPostVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (model.VatNumber != null && (model.VatNumber.Length < 0 || model.VatNumber.Length > 15 || model.VatNumber.Length != 15))
            {
                ModelState.AddModelError(nameof(model.VatNumber), "VatNumber must be 15 characters.");
                return BadRequest(ModelState);
            }

            var company = new Company
            {
                Name = model.Name,
                Email = model.Email,
                BusinessCategory = model.BusinessCategory,
                Number = model.Number,
                Token = model.Token,
                CountryCode = model.CountryCode,
                VatNumber = model.VatNumber,
                Industry = model.Industry,
                UnitName = model.UnitName,
                ArchiveType = model.ArchiveType,
                Environment = model.Environment,
                ArchiveConfig = model.ArchiveConfig,
                ContractStart = model.ContractStart,
                ContractEnd = model.ContractEnd,
                Address = model.Address,
                CompanyIsActive = model.CompanyIsActive,

                AdditionalStreetAddress = model.AdditionalStreetAddress,
                BuildingNumber = model.BuildingNumber,
                CityName = model.CityName,
                IdentityNumber = model.IdentityNumber,
                IdentityType = model.IdentityType,
                DistrictName = model.DistrictName,
                PostalCode = model.PostalCode,
                StreetName = model.StreetName,

            };

            _repository.Add(company);
            await _repository.SaveChanges();


            return Ok(new APIResponse<object>
            {
                IsError = true,
                Message = "",
                data = new
                {
                    company.Name,
                    company.Email,
                    company.BusinessCategory,
                    company.Number,
                    company.Token,
                    company.CountryCode,
                    company.VatNumber,
                    company.Industry,
                    company.UnitName,
                    company.ArchiveType,
                    company.Environment,
                    company.ArchiveConfig,
                    company.ContractStart,
                    company.ContractEnd,
                    company.Address,
                    company.CompanyIsActive,

                    company.AdditionalStreetAddress,
                    company.BuildingNumber,
                    company.CityName,
                    company.IdentityNumber,
                    company.IdentityType,
                    company.DistrictName,
                    company.PostalCode,
                    company.StreetName
                },

            });
        }

        // PUT api/<ValuesController>/5
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(int id, [FromBody] CompanyPutVM model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var company = await this._repository.Get(id).FirstOrDefaultAsync();

            if (company != null)
            {
                company.Name = model.Name;
                company.Email = model.Email;
                company.BusinessCategory = model.BusinessCategory;
                company.Number = model.Number;
                company.Token = model.Token;
                company.CountryCode = model.CountryCode;
                company.VatNumber = model.VatNumber;
                company.Industry = model.Industry;
                company.UnitName = model.UnitName;
                company.ArchiveType = model.ArchiveType;
                company.Environment = model.Environment;
                company.ArchiveConfig = model.ArchiveConfig;
                company.ContractStart = model.ContractStart;
                company.ContractEnd = model.ContractEnd;
                company.Address = model.Address;
                company.CompanyIsActive = model.CompanyIsActive;

                company.AdditionalStreetAddress = model.AdditionalStreetAddress;
                company.BuildingNumber = model.BuildingNumber;
                company.CityName = model.CityName;
                company.IdentityNumber = model.IdentityNumber;
                company.IdentityType = model.IdentityType;
                company.DistrictName = model.DistrictName;
                company.PostalCode = model.PostalCode;
                company.StreetName = model.StreetName;


            }
            this._repository.Update(company);
            await this._repository.SaveChanges();


            return Ok(new APIResponse<object>
            {
                IsError = false,
                Message = "",
            });

            return NotFound();

        }


        // DELETE api/<ValuesController>/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var company = await this._repository.Get(id).FirstOrDefaultAsync();
            if (company != null)
            {
                company.IsActive = false;

                await this._repository.SaveChanges();

                return Ok(new APIResponse<object>
                {
                    IsError = false,
                    Message = "",
                });

            }

            return NotFound();
        }


    }
}