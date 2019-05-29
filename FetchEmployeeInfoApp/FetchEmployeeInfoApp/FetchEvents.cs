using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using FetchEmployeeInfoApp.Models.Queue;
using FetchEmployeeInfoApp.Models.Table;
using FetchEmployeeInfoApp.Models.Graph;
using System.Threading.Tasks;
using System.Net.Http.Headers;
using FetchEmployeeInfoApp.Utils;
using System.Net;
using Microsoft.Graph;

namespace FetchEmployeeInfoApp
{
    public static class FetchEvents
    {
        //Tips: https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static HttpClient graphHttpClient = new HttpClient();
        private static string accessToken;
        private static readonly int sleepInterval = 5000;

        [FunctionName(nameof(FetchEvents))]
        public static async Task Run(
            [QueueTrigger(Settings.eventQueueName, Connection = "")]CalendarSyncRequest inputQueueItem,
            [Queue(Settings.eventQueueName, Connection = "")] ICollector<CalendarSyncRequest> pagingQueueItems,
            [Table(Settings.eventTableName, Connection = "")]ICollector<EventEntity> eventEntities,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {inputQueueItem}");

            //If app doesn't have access token yet, fetch it from Azure AD
            if (string.IsNullOrEmpty(accessToken)) accessToken = await AccessTokenHelper.FetchAccessToken();

            /*
             * Create Http Request
             * In usual we use DefaultRequestHeaders to add http request header. Because many Azure Functions instance will use same httpClient, it will make conflict to handle it.
             * For resolving this, we create HttpRequestMessage for each request.
             * https://stackoverflow.com/questions/23521626/modify-request-headers-per-request-c-sharp-httpclient-pcl
             * */
            string requestQuery = string.IsNullOrEmpty(inputQueueItem.Url) ? CreateRequestQuery(inputQueueItem) : inputQueueItem.Url;
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(requestQuery),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await graphHttpClient.SendAsync(request);
            if (response.StatusCode == HttpStatusCode.Unauthorized)
            {
                //When token expire after 60 min, we need to get new token
                accessToken = await AccessTokenHelper.FetchAccessToken();
                //App will re-try with same queue message
                pagingQueueItems.Add(inputQueueItem);
            }
            else if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                System.Threading.Thread.Sleep(sleepInterval);
                pagingQueueItems.Add(inputQueueItem);
            }
            else if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<CalendarViewResponse>();

                //Pass @odata.nextlink to storage queue for requesting MS graph with multiple Azure Functions node
                if (!string.IsNullOrEmpty(responseData.odatanextLink)) pagingQueueItems.Add(new CalendarSyncRequest()
                {
                    UserId = inputQueueItem.UserId,
                    Url = responseData.odatanextLink,
                    Start = DateTime.Now.AddDays(-1),
                    End = DateTime.Now
                });

                foreach (Event eventData in responseData.value)
                {
                    eventEntities.Add(new EventEntity(eventData, inputQueueItem.UserId));
                }
            }
        }

        private static string CreateRequestQuery(CalendarSyncRequest inputQueueItem)
        {
            return $"https://graph.microsoft.com/v1.0/users/{inputQueueItem.UserId}/calendarView?startDateTime={inputQueueItem.Start.ToString("s")}&endDateTime={inputQueueItem.End.ToString("s")}&$select=iCalUId,subject,location,organizer,onlineMeetingUrl,start,end&$filter=type eq \'singleInstance\'";
        }
    }
}
