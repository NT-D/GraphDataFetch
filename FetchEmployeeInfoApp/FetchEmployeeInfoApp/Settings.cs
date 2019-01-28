using System;

namespace FetchEmployeeInfoApp
{
    public static class Settings
    {
        //Queue names for request
        public const string eventQueueName = "event-sync";
        public const string activityReportQueueName = "activity-report-sync";

        //Tables for storing data
        public const string eventTableName = "eventTable";

        //Blob containers for reports
        //Using json payload binding https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings#binding-expressions---json-payloads
        public const string ReportBlob = "reports/{TypeString}/{DateTime}.csv";

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
