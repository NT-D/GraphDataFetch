using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using FetchEmployeeInfoApp.Utils;
using FetchEmployeeInfoApp.Models.Queue;
using System.Net;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;

namespace FetchEmployeeInfoApp
{
    public static class FetchActivityUserDetailReport
    {
        //Tips: https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static HttpClient graphHttpClient = new HttpClient();
        private static HttpClient downloadClient = new HttpClient();

        //Setup storage client client
        private static CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Settings.connectionString);
        private static CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

        private static string accessToken;

        [FunctionName(nameof(FetchActivityUserDetailReport))]
        public static async Task Run(
            [QueueTrigger(Settings.activityReportQueueName, Connection = "")]ActivityReportRequest inputQueueMessage,
            [Queue(Settings.activityReportQueueName, Connection = "")] ICollector<ActivityReportRequest> retryQueueMessages,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {inputQueueMessage}");
            //We need YYYY-MM-DD foramt
            string todayStringUtc = DateTime.Today.ToString("yyyy-MM-dd");

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await AccessTokenHelper.FetchAccessToken();
            }

            //Generate HTTP Request
            string requestQuery = GenerateReportUrl(inputQueueMessage.Type, todayStringUtc);
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
                    await UploadReport(inputQueueMessage.TypeString, downloadResponse, todayStringUtc);
                }
                //If we got 427 status (TooManyRequests), we need to handle Throttling. https://docs.microsoft.com/en-us/graph/throttling
                else if (downloadResponse.StatusCode == HttpStatusCode.TooManyRequests)
                {
                    TimeSpan sleepTime;
                    if (downloadResponse.Headers.RetryAfter.Delta.HasValue)
                    {
                        //Need to handle Nullable type https://docs.microsoft.com/en-us/dotnet/csharp/programming-guide/nullable-types/index
                        sleepTime = downloadResponse.Headers.RetryAfter.Delta.Value;
                    }
                    else
                    {
                        //Sleep 10 sec at 1st time, then speel 10 + 10n after 2nd time
                        sleepTime = new TimeSpan(0, 0, 10 + inputQueueMessage.RetryCount * 10);
                    }
                    Thread.Sleep(sleepTime);
                    retryQueueMessages.Add(new ActivityReportRequest(inputQueueMessage.Type, inputQueueMessage.RetryCount++));
                }
                else
                {
                    Console.WriteLine($"Response Status: {downloadResponse.StatusCode}");
                    Console.WriteLine($"Error Reason: {downloadResponse.ReasonPhrase}");
                    throw new Exception();//Throw exception and re-try with queue (max.5)
                }
            }

        }

        private static string GenerateReportUrl(ReportType type, string utcToday)
        {
            switch (type)
            {
                case ReportType.Exo:
                    return $"https://graph.microsoft.com/v1.0/reports/getEmailActivityUserDetail(date={utcToday})";
                case ReportType.OneDrive:
                    return $"https://graph.microsoft.com/v1.0/reports/getOneDriveActivityUserDetail(date={utcToday})";
                case ReportType.Teams:
                    return $"https://graph.microsoft.com/v1.0/reports/getTeamsUserActivityUserDetail(date={utcToday})";
                case ReportType.Sfb:
                default:
                    return $"https://graph.microsoft.com/v1.0/reports/getSkypeForBusinessActivityUserDetail(date={utcToday})";
            }
        }

        private static async Task UploadReport(string reportTypeString, HttpResponseMessage downloadResponse, string utcToday)
        {
            CloudBlobContainer blobContainer = blobClient.GetContainerReference(Settings.reportContainer);
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference($"{reportTypeString}/{utcToday}.csv");

            var downloadedReport = await downloadResponse.Content.ReadAsStringAsync();
            await blob.UploadTextAsync(downloadedReport);
        }
    }
}
