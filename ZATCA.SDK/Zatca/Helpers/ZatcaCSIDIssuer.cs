using ZATCA.SDK.Helpers.Zatca.Models;
using ZATCA.SDK.Helpers.Zatca.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using ZATCA.DAL.Repositories.Abstraction;

namespace ZATCA.SDK.Helpers.Zatca.Helpers
{
    public class ZatcaCSIDIssuer : IZatcaCSIDIssuer
    {
        private readonly IZatcaAPICaller _zatcaAPICaller;
        private readonly IZatcaCsrReader _csrReader;
        private readonly IXmlInvoiceGenerator _xmlGenerator;
        private readonly IInvoiceSigner _signer;
        private readonly ICompanyRepository _companyRepository;
        string vat;
        public ZatcaCSIDIssuer(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
            _zatcaAPICaller = new ZatcaAPICaller();
            _csrReader = new ZatcaCsrReader(companyRepository);
            _xmlGenerator = new XmlInvoiceGenerator();
            _signer = new InvoiceSigner();

        }
        //public ZatcaCSIDIssuer(IZatcaAPICaller zatcaAPICaller, IZatcaCsrReader csrReader
        //    , IXmlInvoiceGenerator xmlGenerator, IInvoiceSigner signer)
        //{
        //    this._zatcaAPICaller = zatcaAPICaller;
        //    this._csrReader = csrReader;
        //    this._xmlGenerator = xmlGenerator;
        //    this._signer = signer;
        //}
        public Supplier Supplier { get; set; }
        public async Task<CSIDResultModel> OnboardingCSIDAsync(InputCSIDOnboardingModel model)
        {
            if (string.IsNullOrEmpty(model.csr))
            {
                return new CSIDResultModel { ErrorMessage= "Csr Not Found"};
            }
            var companyCsr = model.csr;
            this.Supplier = model.Supplier;
            if (!string.IsNullOrEmpty(companyCsr))
            {
                var complianceResult = await _zatcaAPICaller.CompleteComplianceCSIDAsync(new InputComplianceModel
                {
                    csr = companyCsr
                },
                model.otp);

                if (complianceResult == null)
                {
                    return new CSIDResultModel { ErrorMessage = ZatcaHttpClient.LastErrorMessage };
                }

                if (!string.IsNullOrEmpty(complianceResult.BinarySecurityToken))
                {
                    SharedData.UserName = complianceResult.BinarySecurityToken;
                    SharedData.Secret = complianceResult.Secret;

                    var csrResult = _csrReader.GetCsrInvoiceType(companyCsr);
                    if (csrResult != null)
                    {
                        vat = csrResult.VatRegNumber;

                        //310175397400003
                        if (csrResult.StandardAllowed)
                        {
                            var invoiceModel = GetInvoiceModel(InvoiceType.Standard, InvoiceTypeCode.Invoice, TransactionTypeCode.Standard);
                            var invoiceSigned = GetSignedXmlResult(invoiceModel);
                            var debitModel = GetInvoiceModel(InvoiceType.Standard, InvoiceTypeCode.Debit, TransactionTypeCode.Standard);
                            var debitSigned = GetSignedXmlResult(debitModel);
                            var creditModel = GetInvoiceModel(InvoiceType.Standard, InvoiceTypeCode.Credit, TransactionTypeCode.Standard);
                            var creditSigned = GetSignedXmlResult(creditModel);

                            foreach (var m in new[] { invoiceSigned, debitSigned, creditSigned })
                            {
                                if (m != null)
                                {
                                    var invoiceCompliance = await _zatcaAPICaller.PerformComplianceCheckAsync(new InputInvoiceModel
                                    {
                                        invoice = m.InvoiceAsBase64,
                                        invoiceHash = m.InvoiceHash,
                                        uuid = m.UUID
                                    });
                                    if (invoiceCompliance.ClearanceStatus != "CLEARED")
                                    {
                                        return new CSIDResultModel { ErrorMessage = "Clearance Status: Not Cleared" };
                                    }
                                }
                                else
                                {
                                    return new CSIDResultModel { ErrorMessage = "Issue in Server" };

                                }
                            }
                        }
                       

                        if (csrResult.SimplifiedAllowed)
                        {
                            var invoiceModel = GetInvoiceModel(InvoiceType.Simplified, InvoiceTypeCode.Invoice, TransactionTypeCode.Simplified);
                            var invoiceSigned = GetSignedXmlResult(invoiceModel);
                            var debitModel = GetInvoiceModel(InvoiceType.Simplified, InvoiceTypeCode.Debit, TransactionTypeCode.Simplified);
                            var debitSigned = GetSignedXmlResult(debitModel);
                            var creditModel = GetInvoiceModel(InvoiceType.Simplified, InvoiceTypeCode.Credit, TransactionTypeCode.Simplified);
                            var creditSigned = GetSignedXmlResult(creditModel);

                            foreach (var m in new[] { invoiceSigned, debitSigned, creditSigned })
                            {
                                if (m != null)
                                {
                                    var invoiceCompliance = await _zatcaAPICaller.PerformComplianceCheckAsync(new InputInvoiceModel
                                    {
                                        invoice = m.InvoiceAsBase64,
                                        invoiceHash = m.InvoiceHash,
                                        uuid = m.UUID
                                    });
                                    if (invoiceCompliance.ReportingStatus != "REPORTED")
                                    {
                                        return null;
                                    }
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        var certResult = await _zatcaAPICaller.OnboardingCSIDAsync(new InputCSIDModel
                        {
                            compliance_request_id = complianceResult.RequestId.ToString()
                        });

                        if (certResult.RequestId > 0)
                        {
                            X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(certResult.BinarySecurityToken));
                            if (certificate != null)
                            {
                                return new CSIDResultModel
                                {
                                    
                                    Secret = certResult.Secret,
                                    Certificate = certResult.BinarySecurityToken,
                                    StartedDate = certificate.NotBefore,
                                    ExpiredDate = certificate.NotAfter,
                                };
                            }
                        }
                    }
                    else
                    {
                        return new CSIDResultModel { ErrorMessage = " Csr is Empty" };
                    }
                }

                return new CSIDResultModel
                {
                    ErrorMessage=complianceResult.Errors,
                    Certificate = complianceResult.BinarySecurityToken,
                    Secret = complianceResult.Secret,
                    ExpiredDate = DateTime.Now.AddYears(5),
                    StartedDate = DateTime.Now,
                };
            }
            return new CSIDResultModel { ErrorMessage= "Company Csr is Empty"};
        }
        private InvoiceDataModel GetInvoiceModel(InvoiceType invoiceType, InvoiceTypeCode invoiceTypeCode, string transactionTypeCode)
        {

            var companyData = _companyRepository.Get().Where(p => p.VatNumber == vat).FirstOrDefault();

            var model = new InvoiceDataModel
            {
                InvoiceNumber = GetNextInvoiceNumber(),
                InvoiceType = (int)invoiceType,
                InvoiceTypeCode = (int)invoiceTypeCode,
                Id = Guid.NewGuid().ToString(),
                Order = 2,
                TransactionTypeCode = transactionTypeCode,
                Lines = new List<LineItem>
                {
                    new LineItem {  Index = 1, ProductName = "T-Shirt"    , Quantity = 2, NetPrice = 20.55 , Tax = 15},
                    new LineItem {  Index = 2, ProductName = "LCD Screen" , Quantity = 1, NetPrice = 2499.99, Tax = 15},
                },
                PaymentMeansCode = 10,
                //Supplier = this.Supplier,
                Supplier = new Supplier
                {

                    //SellerName = "SAUDI ERICSSON Communications",
                    //SellerTRN = "300056986300003",
                    //AdditionalStreetAddress = "6913",
                    //BuildingNumber = "6913",
                    //CityName = "Adh Dhubbat",
                    //IdentityNumber = "1010032572",
                    //IdentityType = "CRN",
                    //CountryCode = "SA",
                    //DistrictName = "Adh Dhubbat Dist",
                    //PostalCode = "12623",
                    //StreetName = "Salah Ad Din Al Ayyubi Rd",

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
                    CustomerName = "Saleh Saleh",
                    IdentityNumber = "311111111111113",
                    IdentityType = "NAT",
                    VatRegNumber = "300000000000003",
                    StreetName = "Makka",
                    BuildingNumber = "1111",
                    ZipCode = "12345",
                    CityName = "Al Riyadh",
                    DistrictName = "Al Olia",
                    RegionName = "Al Riyadh"
                },
                IssueDate = "2022-09-26",
                IssueTime = "17:00:00",
                PreviousInvoiceHash = GetPreviousInvoiceHash()
            };

            if (invoiceType == InvoiceType.Simplified)
            {
                model.Customer = null;
            }

            if (invoiceTypeCode == InvoiceTypeCode.Credit || invoiceTypeCode == InvoiceTypeCode.Debit)
            {
                model.Notes = "Test Notes";
                model.ReferenceId = "ESM1029"; //Sample Invoice Number
            }

            return model;
        }

        private string GetPreviousInvoiceHash()
        {
            return "NWZlY2ViNjZmZmM4NmYzOGQ5NTI3ODZjNmQ2OTZjNzljMmRiYzIzOWRkNGU5MWI0NjcyOWQ3M2EyN2ZiNTdlOQ==";
        }

        private string GetNextInvoiceNumber()
        {
            //Sample Number
            return "INV201";
        }

        private ZatcaResult GetSignedXmlResult(InvoiceDataModel model)
        {
            // 01 - Generate XML 
            var xmlStream = ZatcaUtility.ReadInternalEmbededResourceStream(XslSettings.Embeded_InvoiceXmlFile);

            var invoiceXml = _xmlGenerator.GenerateInvoiceAsXml(xmlStream, model);

            // 02- Sign XML
            var signingResult = _signer.Sign(invoiceXml);

            // 03- Report to API
            if (signingResult.IsValid)
            {
                return signingResult;
            }

            return null;
        }

        public async Task<CSIDResultModel> RenewCSIDAsync(InputCSIDRenewingModel model)
        {
            if (string.IsNullOrEmpty(model.csr))
            {
                return null;
            }
            if (!string.IsNullOrEmpty(model.csr))
            {
                var renewalResult = await _zatcaAPICaller.RenewalCSIDAsync(new InputComplianceModel
                {
                    csr = model.csr
                }, model.otp);


                X509Certificate2 certificate = new X509Certificate2(Convert.FromBase64String(renewalResult.BinarySecurityToken));
                if (certificate != null)
                {
                    return new CSIDResultModel
                    {
                        Secret = renewalResult.Secret,
                        Certificate = renewalResult.BinarySecurityToken,
                        StartedDate = certificate.NotBefore,
                        ExpiredDate = certificate.NotAfter,
                    };
                }
            }
            return null;
        }
    }
}
