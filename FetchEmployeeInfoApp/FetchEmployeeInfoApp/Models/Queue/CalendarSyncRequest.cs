using System;

namespace FetchEmployeeInfoApp.Models.Queue
{
    public class CalendarSyncRequest
    {
        public string UserId { get; set; }
        public string Url { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
