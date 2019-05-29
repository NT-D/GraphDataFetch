using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Graph;
using Newtonsoft.Json;

namespace FetchEmployeeInfoApp.Models.Graph
{
    public class UserResponse
    {
        [JsonProperty(propertyName: "@odata.nextLink")]
        public string NextLink { get; set; }
        public User[] Users { get; set; }
    }
}
