using System;
using System.Collections.Generic;

namespace FetchEmployeeInfoApp
{
    public static class Settings
    {
        public static readonly string connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage", EnvironmentVariableTarget.Process);

        //Blobs for report data
        public const string reportContainer = "reports";
        public static readonly List<string> containersForFunction = new List<string>() { reportContainer };

        //Tables for storing data
        public const string eventTableName = "eventTable";
        public const string userTableName = "userTable";
        public static readonly List<string> tablesForFuction = new List<string>() { eventTableName, userTableName };

        //Queue names for request
        public const string userQueueName = "user-sync";
        public const string eventQueueName = "event-sync";
        public const string activityReportQueueName = "activity-report-sync";
        public static readonly List<string> queuesForFunction = new List<string>() { userQueueName, eventQueueName, activityReportQueueName };

        /* How to debug
         * Please set your local.settings.json with following keys.
         * Reference1: https://docs.microsoft.com/en-us/azure/azure-functions/functions-run-local#local-settings-file
         * Reference2: https://docs.microsoft.com/en-us/azure/azure-functions/functions-dotnet-class-library#environment-variables
         */
        public static readonly string ClientId = Environment.GetEnvironmentVariable("ClientId", EnvironmentVariableTarget.Process);
        public static readonly string ClientSecret = Environment.GetEnvironmentVariable("ClientSecret", EnvironmentVariableTarget.Process);
        public static readonly string TenantId = Environment.GetEnvironmentVariable("TenantId", EnvironmentVariableTarget.Process);
    }
}
