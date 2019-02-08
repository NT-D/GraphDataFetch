using System;
using System.Threading;
using FetchEmployeeInfoApp.Models.Queue;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FetchEmployeeInfoApp
{
    public static class StartTimer
    {
        private static int sleepInterval = 10000;

        [FunctionName(nameof(StartTimer))]
        public static void Run(
            [TimerTrigger("0 */5 * * * *")]TimerInfo myTimer,
            [Queue(Settings.activityReportQueueName, Connection = "")]ICollector<ActivityReportRequest> reportQueueMessages,
            ILogger log)
        {
            log.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            //Send report download request
            foreach (ReportType type in Enum.GetValues(typeof(ReportType)))
            {
                reportQueueMessages.Add(new ActivityReportRequest(type));
                //Because report file is sometimes huge, we send each request after sleep
                Thread.Sleep(sleepInterval);
            }
        }
    }
}
