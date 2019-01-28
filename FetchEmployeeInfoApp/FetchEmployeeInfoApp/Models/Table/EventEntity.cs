using System;
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
    }
}
