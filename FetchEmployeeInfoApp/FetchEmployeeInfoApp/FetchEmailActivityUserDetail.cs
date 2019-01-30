using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using FetchEmployeeInfoApp.Utils;
using FetchEmployeeInfoApp.Models.Queue;
using System.IO;

namespace FetchEmployeeInfoApp
{
    public static class FetchEmailActivityUserDetail
    {
        //Tips: https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static HttpClient graphHttpClient = new HttpClient();
        private static HttpClient downloadClient = new HttpClient();
        private static string accessToken;

        [FunctionName(nameof(FetchEmailActivityUserDetail))]
        public static async Task Run(
            [QueueTrigger(Settings.activityReportQueueName, Connection = "")]ActivityReportRequest inputQueueMessage,
            [Blob(Settings.ReportBlob, FileAccess.Write)] TextWriter reportFile,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {inputQueueMessage}");

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await AccessTokenHelper.FetchAccessToken();
            }

            //Generate HTTP Request
            string requestQuery = GenerateReportUrl(inputQueueMessage.Type, inputQueueMessage.Period);
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(requestQuery),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            //Send Http Request and get report url
            HttpResponseMessage response = await graphHttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                string downloadUrl = response.RequestMessage.RequestUri.ToString();
                //Console.WriteLine(downloadUrl);

                //Download report(csv file) and save it to blob storage
                var downloadResponse = await downloadClient.GetAsync(downloadUrl);
                if (downloadResponse.IsSuccessStatusCode)
                {
                    var downloadedReport = await downloadResponse.Content.ReadAsStringAsync();
                    await reportFile.WriteAsync(downloadedReport);
                }
                else
                {
                    Console.WriteLine($"Response Status: {downloadResponse.StatusCode}");
                    Console.WriteLine($"Error Reason: {downloadResponse.ReasonPhrase}");
                    throw new Exception();//Throw exception and re-try with queue (max.5)
                }
            }

        }

        private static string GenerateReportUrl(ReportType type, ReportPeriod period)
        {
            string reportPeriod = period.ToString("g");//Input enum as string ex. D7
            switch (type)
            {
                case ReportType.Exo:
                    return $"https://graph.microsoft.com/v1.0/reports/getEmailActivityUserDetail(period='{reportPeriod}')";
                case ReportType.OneDrive:
                    return $"https://graph.microsoft.com/v1.0/reports/getOneDriveActivityUserDetail(period='{reportPeriod}')";
                case ReportType.Teams:
                    return $"https://graph.microsoft.com/v1.0/reports/getTeamsUserActivityUserDetail(period='{reportPeriod}')";
                case ReportType.Sfb:
                default:
                    return $"https://graph.microsoft.com/v1.0/reports/getSkypeForBusinessActivityUserDetail(period='{reportPeriod}')";
            }
        }
    }
}
