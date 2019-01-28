using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using FetchEmployeeInfoApp.Models.Queue;
using FetchEmployeeInfoApp.Models.Table;
using FetchEmployeeInfoApp.Models.Graph;
using System.Threading.Tasks;
using System.Net.Http.Headers;

namespace FetchEmployeeInfoApp
{
    public static class FetchEvents
    {
        //Tips: https://docs.microsoft.com/en-us/azure/azure-functions/manage-connections
        private static HttpClient graphHttpClient = new HttpClient();

        [FunctionName(nameof(FetchEvents))]
        public static async Task Run(
            [QueueTrigger(Settings.eventQueueName, Connection = "")]CalendarSyncStartRequest inputQueueItem,
            [Queue(Settings.eventQueueName, Connection = "")] ICollector<CalendarSyncStartRequest> pagingQueueItem,
            [Table(Settings.eventTableName, Connection = "")]ICollector<EventEntity> events,
            ILogger log)
        {
            log.LogInformation($"C# Queue trigger function processed: {inputQueueItem}");

            /*
             * Create Http Request
             * In usual we use DefaultRequestHeaders to add http request header. Because many Azure Functions instance will use same httpClient, it will make conflict to handle it.
             * For resolving this, we create HttpRequestMessage for each request.
             * https://stackoverflow.com/questions/23521626/modify-request-headers-per-request-c-sharp-httpclient-pcl
             * */
            string requestQuery = String.IsNullOrEmpty(inputQueueItem.requestUrl) ? CreateRequestQuery(inputQueueItem) : inputQueueItem.requestUrl;
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(requestQuery),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", inputQueueItem.accessToken);

            //Send http request to Microsoft graph
            HttpResponseMessage response = await graphHttpClient.SendAsync(request);
            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsAsync<CalendarViewResponse>();
                foreach (var eventData in responseData.value)
                {
                    var entity = new EventEntity()
                    {
                        PartitionKey = inputQueueItem.userId,
                        RowKey = eventData.ICalUId,
                        Subject = eventData.Subject,
                        Location = eventData.Location.DisplayName,
                        LocationEmailAddress = eventData.Location.LocationEmailAddress,
                        OwnerEmailAddress = eventData.Organizer.EmailAddress.Address,
                        IsOnlineMeetingUrl = !string.IsNullOrEmpty(eventData.OnlineMeetingUrl),
                        //TODO: Need to convert to UTC
                        UtcStartTime = DateTime.Parse(eventData.Start.DateTime),
                        UtcEndTime = DateTime.Parse(eventData.End.DateTime)
                    };
                    events.Add(entity);
                }
                if (!string.IsNullOrEmpty(responseData.odatanextLink)) pagingQueueItem.Add(new CalendarSyncStartRequest() { userId = inputQueueItem.userId, accessToken = inputQueueItem.accessToken, requestUrl = responseData.odatanextLink });
            }
        }

        private static string CreateRequestQuery(CalendarSyncStartRequest inputQueueItem)
        {
            return $"https://graph.microsoft.com/v1.0/me/calendarView?startDateTime={inputQueueItem.start.ToString("s")}&endDateTime={inputQueueItem.end.ToString("s")}&$select=iCalUId,subject,location,organizer,onlineMeetingUrl,start,end&$filter=type eq \'singleInstance\'";
        }
    }
}
