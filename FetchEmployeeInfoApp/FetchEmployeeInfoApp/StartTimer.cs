using System;
using System.Threading;
using System.Threading.Tasks;
using FetchEmployeeInfoApp.Models.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Table;
using Microsoft.WindowsAzure.Storage.Queue;

namespace FetchEmployeeInfoApp
{
    public static class StartTimer
    {
        private static readonly int sleepInterval = 10000;
        private static readonly CloudStorageAccount storageAccount = CloudStorageAccount.Parse(Environment.GetEnvironmentVariable("AzureWebJobsStorage"));
        private static readonly CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();
        private static readonly CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
        private static readonly CloudQueueClient queueClient = storageAccount.CreateCloudQueueClient();

        [FunctionName(nameof(StartTimer))]
        public async static Task Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Queue(Settings.userQueueName, Connection = "")]ICollector<UserSyncRequest> userQueueItems,
            [Queue(Settings.activityReportQueueName, Connection = "")]ICollector<ActivityReportRequest> reportQueueMessages,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            //Initialize storage
            await InitializeAzureStorage();

            //Send queue message for fetching O365 data
            userQueueItems.Add(new UserSyncRequest() { Url = string.Empty });

            //Send report download request
            foreach (ReportType type in Enum.GetValues(typeof(ReportType)))
            {
                reportQueueMessages.Add(new ActivityReportRequest(type));
                //Because report file is sometimes huge, we send each request after sleep
                Thread.Sleep(sleepInterval);
            }
        }

        private static async Task InitializeAzureStorage()
        {
            foreach(string containerName in Settings.containersForFunction)
            {
                CloudBlobContainer blobContainer = blobClient.GetContainerReference(Settings.reportContainer);
                await blobContainer.CreateIfNotExistsAsync();
            }

            foreach (string tableName in Settings.tablesForFuction)
            {
                CloudTable table = tableClient.GetTableReference(tableName);
                await table.CreateIfNotExistsAsync();
            }

            foreach(string queueName in Settings.queuesForFunction)
            {
                CloudQueue queue = queueClient.GetQueueReference(queueName);
                await queue.CreateIfNotExistsAsync();
            }
        }
    }
}
