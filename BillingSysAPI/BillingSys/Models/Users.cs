using System;
using Newtonsoft.Json;


namespace FunctionApp1.Models
{
    public class Users
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }

        public string PartitionKey { get; set; }

        public string SubId { get; set; }
    }
}
