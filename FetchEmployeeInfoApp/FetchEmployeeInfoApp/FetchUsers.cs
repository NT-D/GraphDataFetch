using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using FetchEmployeeInfoApp.Models.Queue;
using FetchEmployeeInfoApp.Models.Table;
using FetchEmployeeInfoApp.Utils;
using FetchEmployeeInfoApp.Models.Graph;
using Microsoft.Graph;
using System.Net.Http.Headers;

namespace FetchEmployeeInfoApp
{
    public static class FetchUsers
    {
        private static HttpClient graphClient = new HttpClient();
        private static string accessToken;
        private static readonly string defaultUserRequestUrl = "https://graph.microsoft.com/v1.0/users?$select=id,city,displayname,officelocation,country,department,preferredlanguage,userprincipalname&$top=999";
        private static readonly int sleepInterval = 5000;

        [FunctionName(nameof(FetchUsers))]
        public static async Task Run(
            [QueueTrigger(Settings.userQueueName, Connection = "")] UserSyncRequest inputQueueItem,
            [Queue(Settings.userQueueName, Connection = "")]ICollector<UserSyncRequest> pagingQueueItems,
            [Table(Settings.userTableName, Connection = "")]ICollector<UserEntity> userEntities,
            [Queue(Settings.eventQueueName, Connection = "")]ICollector<CalendarSyncRequest> calendarQueueItems,
            ILogger log)
        {
            log.LogInformation("Fetch user info started");

            //If app doesn't have access token yet, fetch it from Azure AD
            if (string.IsNullOrEmpty(accessToken)) accessToken = await AccessTokenHelper.FetchAccessToken();

            //If app get queue message without url, app will use default query. If not, app will use url (nextlink)
            string graphRequestUrl = string.IsNullOrEmpty(inputQueueItem.Url) ? defaultUserRequestUrl : inputQueueItem.Url;

            //Create http request message with access token
            var request = new HttpRequestMessage()
            {
                RequestUri = new Uri(graphRequestUrl),
                Method = HttpMethod.Get
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            HttpResponseMessage response = await graphClient.SendAsync(request);
            log.LogInformation($"Reponse code is: {response.StatusCode}");

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
                var responseData = await response.Content.ReadAsAsync<UserResponse>();
                //Pass @odata.nextlink to storage queue for requesting MS graph with multiple Azure Functions node
                if (!string.IsNullOrEmpty(responseData.NextLink)) pagingQueueItems.Add(new UserSyncRequest() { Url = responseData.NextLink });

                foreach (User userData in responseData.Users)
                {
                    //Save user data to Storage Table
                    userEntities.Add(new UserEntity(userData));
                    //Send queue message for fetching calendar items
                    calendarQueueItems.Add(new CalendarSyncRequest(userData.Id));
                }
            }
        }
    }
}
