using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace FunctionApp1.Models
{
    public class Subscription
    {
        [JsonProperty(PropertyName="id")]
        public string Id { get; set; }
        public int Price { get; set; }

        public string PartitionKey { get; set; }
    }
}
