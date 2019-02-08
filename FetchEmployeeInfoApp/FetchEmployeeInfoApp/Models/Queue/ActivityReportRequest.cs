namespace FetchEmployeeInfoApp.Models.Queue
{
    public class ActivityReportRequest
    {
        public ReportType Type { get; set; }

        //Please take care about to change this property name because we use json payload binding
        //https://docs.microsoft.com/en-us/azure/azure-functions/functions-triggers-bindings#binding-expressions---json-payloads
        public string TypeString { get; set; }

        public int RetryCount { get; set; }

        public ActivityReportRequest() { }

        //retryCount is 0 if we don't pass retryCount argument to make instance
        public ActivityReportRequest(ReportType type, int retryCount = 0)
        {
            Type = type;
            TypeString = type.ToString("g");
            RetryCount = retryCount;
        }
    }

    public enum ReportType
    {
        Sfb,
        Exo,
        OneDrive,
        Teams
    }
}
