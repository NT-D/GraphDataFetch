using System;

namespace FetchEmployeeInfoApp.Models.Queue
{
    public class CalendarSyncRequest
    {
        public string UserId { get; set; }
        public string Url { get; set; }
        public DateTime Start { get; set; }
        public DateTime End { get; set; }

        public CalendarSyncRequest(string userId)
        {
            UserId = userId;
            Url = string.Empty;
            Start = DateTime.Now.AddDays(-1);
            End = DateTime.Now;
        }

        public CalendarSyncRequest(string userId, string url)
        {
            UserId = userId;
            Url = url;
            Start = DateTime.Now.AddDays(-1);
            End = DateTime.Now;
        }

        public CalendarSyncRequest(string userId, string url, DateTime start, DateTime end)
        {
            UserId = userId;
            Url = url;
            Start = start;
            End = end;
        }
    }
}
