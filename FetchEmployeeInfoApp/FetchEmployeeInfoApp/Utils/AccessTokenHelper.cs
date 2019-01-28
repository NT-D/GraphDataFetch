using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using FetchEmployeeInfoApp.Models.Graph;

namespace FetchEmployeeInfoApp.Utils
{
    public static class AccessTokenHelper
    {
        private readonly static int sleepInterval = 5000;
        private static HttpClient tokenClient = new HttpClient();

        public static async Task<string> FetchAccessToken()
        {
            var requestContent = new Dictionary<string, string>();
            requestContent.Add("client_id", Settings.ClientId);
            requestContent.Add("client_secret", Settings.ClientSecret);
            requestContent.Add("scope", "https://graph.microsoft.com/.default");
            requestContent.Add("grant_type", "client_credentials");

            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri($"https://login.microsoftonline.com/{Settings.TenantId}/oauth2/v2.0/token"),
                Method = HttpMethod.Post,
                Content = new FormUrlEncodedContent(requestContent)
            };

            var response = await tokenClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<AccessTokenResponse>();
                return responseData.Access_token;
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                // wait and try                 
                Thread.Sleep(sleepInterval);
                var retryResponse = await tokenClient.SendAsync(request);
                var retryResponseData = await retryResponse.Content.ReadAsAsync<AccessTokenResponse>(); ;
                return retryResponseData.Access_token;
            }
            else
            {
                throw new Exception();
            }
        }
    }
}
