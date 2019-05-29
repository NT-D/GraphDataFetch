using System;
using Microsoft.Graph;
using Microsoft.WindowsAzure.Storage.Table;

namespace FetchEmployeeInfoApp.Models.Table
{
    public class EventEntity : TableEntity
    {
        //PartitionKey will be userid
        //Rowkey will be iCalUId
        public string Subject { get; set; }
        public string Location { get; set; }
        public string LocationEmailAddress { get; set; }
        public string OwnerEmailAddress { get; set; }
        public bool IsOnlineMeetingUrl { get; set; }
        public DateTime UtcStartTime { get; set; }
        public DateTime UtcEndTime { get; set; }

        public EventEntity(Event eventData, string userId)
        {
            PartitionKey = userId;
            RowKey = eventData.ICalUId;
            Subject = eventData.Subject;
            Location = eventData.Location.DisplayName;
            LocationEmailAddress = eventData.Location.LocationEmailAddress;
            OwnerEmailAddress = eventData.Organizer.EmailAddress.Address;
            IsOnlineMeetingUrl = !string.IsNullOrEmpty(eventData.OnlineMeetingUrl);
            UtcStartTime = DateTime.Parse(eventData.Start.DateTime).ToUniversalTime();
            UtcEndTime = DateTime.Parse(eventData.End.DateTime).ToUniversalTime();
        }
    }
}
