/*
 * Author  : Ahmed Moosa
 * Email   : ahmed_moosa83@hotmail.com
 * LinkedIn: https://www.linkedin.com/in/ahmoosa/
 * Date    : 26/9/2022
 */
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using ZATCA.SDK.Helpers.Zatca.Models;
using Newtonsoft.Json;
using Org.BouncyCastle.Asn1.Cmp;
using Org.BouncyCastle.Asn1.Crmf;
using RestSharp;
namespace ZATCA.SDK.Helpers.Zatca
{
    public class ZatcaHttpClient
    {
        public static string LastErrorMessage { get; private set; }

        public static TResult PostAsync<TResult, TInput>(string url, TInput model, IDictionary<string, string> headers, bool requireAuth = false, bool patchHttpMethod = false) where TResult : class
        {
            RestResponse response = null;
            try
            {
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;

                var client = new RestClient(SharedData.APIUrl);
                var req = new RestRequest(url);
                req.AddHeader("Accept-Version", "V2");
                req.AddHeader("Accept-Language", "en");

                req.AddHeader("Accept", "application/json");

                if (headers != null)
                {
                    foreach (var header in headers)
                    {
                        req.AddHeader(header.Key, header.Value);
                    }
                }

                if (requireAuth)
                {
                    req.AddHeader("Authorization", "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes($"{SharedData.UserName}:{SharedData.Secret}")));
                }

                if (patchHttpMethod)
                {
                    req.AddBody(model);
                    req.Method = Method.Patch;
                    response = client.Execute(req);
                }
                else
                {
                    req.Method = Method.Post;
                    req.RequestFormat = DataFormat.Json;
                    req.AddBody(model);
                    response = client.Execute(req);
                }

                var result = JsonConvert.DeserializeObject<TResult>(response.Content);

                //return result;
                if (response.StatusCode == System.Net.HttpStatusCode.SeeOther ||  response.StatusCode == System.Net.HttpStatusCode.InternalServerError || response.StatusCode == System.Net.HttpStatusCode.Unauthorized || (model is InputCSIDModel && response.StatusCode == System.Net.HttpStatusCode.BadRequest))
                {
                    //var errorResponse =  response.Content
                    LastErrorMessage = $"Response: {(int)response.StatusCode}-{response.StatusDescription} , Message : {response.Content}";

                    return null;
                }

                var reportingResult = result as InvoiceModelResult;
                if (reportingResult != null)
                {
                    if (reportingResult.ValidationResults?.ErrorMessages?.Count > 0 || reportingResult.ValidationResults?.WarningMessages?.Count > 0)
                    {
                        LastErrorMessage = JsonConvert.SerializeObject(reportingResult);
                    }
                }
                return result;
            }
            catch (Exception ex)
            {
                if (response != null)
                {
                    var errorRespone = response.Content;
                    if (!string.IsNullOrEmpty(errorRespone))
                    {
                        LastErrorMessage = $" {response.StatusCode} : {response.Content}";
                    }
                }
                return null;
            }
        }
    }
}
