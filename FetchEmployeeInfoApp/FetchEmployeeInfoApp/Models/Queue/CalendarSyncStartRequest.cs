using System;

namespace FetchEmployeeInfoApp.Models.Queue
{
    public class CalendarSyncStartRequest
    {
        public string userId { get; set; }
        public string accessToken { get; set; }
        public string requestUrl { get; set; }
        public DateTime start { get; set; }
        public DateTime end { get; set; }
    }
}
