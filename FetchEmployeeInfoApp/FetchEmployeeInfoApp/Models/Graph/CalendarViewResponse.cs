using Microsoft.Graph;
using Newtonsoft.Json;

namespace FetchEmployeeInfoApp.Models.Graph
{
    public class CalendarViewResponse
    {
        [JsonProperty(propertyName: "@odata.context")]
        public string odatacontext { get; set; }
        [JsonProperty(propertyName: "@odata.nextLink")]
        public string odatanextLink { get; set; }
        public Event[] value { get; set; }

    }
}
