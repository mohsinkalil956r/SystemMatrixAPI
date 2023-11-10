/*
 * Author  : Ahmed Moosa
 * Email   : ahmed_moosa83@hotmail.com
 * LinkedIn: https://www.linkedin.com/in/ahmoosa/
 * Date    : 26/9/2022
 */
using ZATCA.SDK.Helpers.Models;
using ZATCA.SDK.Helpers.Zatca.Models;
using ZATCA.SDK.Helpers.Zatca.Services;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace ZATCA.SDK.Helpers.Zatca.Helpers
{
    public class ZatcaReporter : IZatcaReporter
    {
        private readonly IZatcaAPICaller _apiCaller;
        private readonly IXmlInvoiceGenerator _xmlGenerator;
        private readonly IInvoiceSigner _signer;

        public ZatcaReporter()
        {
            _apiCaller = new ZatcaAPICaller();
            _xmlGenerator = new XmlInvoiceGenerator();
            _signer = new InvoiceSigner();
        }
        public ZatcaReporter(IZatcaAPICaller apiCaller,
            IXmlInvoiceGenerator xmlGenerator,
            IInvoiceSigner signer)
        {
            this._apiCaller = apiCaller;
            this._xmlGenerator = xmlGenerator;
            this._signer = signer;
        }

        public async Task<ZatcaInvoiceReportResult> ReportInvoiceAsync<T>(T model) where T : class
        {
            try
            {
                // 01 - Generate XML 
                var xmlStream = ZatcaUtility.ReadInternalEmbededResourceStream(XslSettings.Embeded_InvoiceXmlFile);
                var invoiceXml = _xmlGenerator.GenerateInvoiceAsXml(xmlStream, model);

                // 02- Sign XML
                var signingResult = _signer.Sign(invoiceXml);

                // 03- Report to Zatca API
                if (signingResult.IsValid)
                {
                    int clearanceStatus = GetClearanceStatus(signingResult);

                    var apiResult = await _apiCaller.ReportSingleInvoiceAsync(new InputInvoiceModel
                    {
                        invoice = signingResult.InvoiceAsBase64,
                        invoiceHash = signingResult.InvoiceHash,
                        uuid = signingResult.UUID,
                    }, clearanceStatus);

                    if (apiResult != null)
                    {
                        // 04- return results
                        return new ZatcaInvoiceReportResult
                        {
                            Success = signingResult.IsSimplified ? apiResult.ReportingStatus == "REPORTED" : apiResult.ClearanceStatus == "CLEARED",
                            Data = new ZatcaInvoiceModel
                            {
                                InvoiceHash = signingResult.InvoiceHash,
                                QrCode = signingResult.IsSimplified ? signingResult.QrCode : getQrCode(apiResult.ClearedInvoice),
                                SignedXml = signingResult.SingedXML,
                                ReportingStatus = signingResult.IsSimplified ? apiResult.ReportingStatus : apiResult.ClearanceStatus,
                                ReportingResult = JsonConvert.SerializeObject(apiResult),
                                SubmissionDate = DateTime.Now,
                                IsReportedToZatca = signingResult.IsSimplified ? apiResult.ReportingStatus == "REPORTED" : apiResult.ClearanceStatus == "CLEARED",
                                InvoiceBase64 = signingResult.InvoiceAsBase64,
                                ErrorMessages = apiResult.ValidationResults.ErrorMessages,
                                WarningMessages = apiResult.ValidationResults.WarningMessages
                            }
                        };
                    }
                }
                return new ZatcaInvoiceReportResult() { Success = false };
            }
            catch (Exception ex)
            {
                return new ZatcaInvoiceReportResult() { Success = false, Message = ex.Message };
            }
        }
        private string getQrCode(string clearedInvoice)
        {
            if (!string.IsNullOrEmpty(clearedInvoice))
            {
                var signedXml = Encoding.UTF8.GetString(Convert.FromBase64String(clearedInvoice));
                if (!string.IsNullOrEmpty(signedXml))
                {
                    var xmlDoc = new XmlDocument();
                    xmlDoc.LoadXml(signedXml);

                    var qrCode = ZatcaUtility.GetNodeInnerText(xmlDoc, XslSettings.QR_CODE_XPATH);
                    return qrCode;
                }
            }
            return null;
        }

        private static void SaveXmlFile(ZatcaResult signingResult)
        {
            var fileName = signingResult.IsSimplified ? "Simplified" : "Standard";
            var pathToSave = $@"C:\Invoice Files2\{fileName}.xml";
            File.WriteAllText(pathToSave, signingResult.SingedXML);
        }

        private int GetClearanceStatus(ZatcaResult signingResult)
        {
            return signingResult.IsSimplified ? 0 : 1;
        }
    }
}
