using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using FunctionApp1.Models;
using Microsoft.Azure.Cosmos;

namespace FunctionApp1
{
    public class SubscriptionFunc
    {
        private readonly IDbAdapter _adapter;
        public SubscriptionFunc(IDbAdapter adp)
        {
            _adapter = adp;
        }

        [FunctionName("Subscription")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "subscribtions/{substr}/")] HttpRequest req, string substr,
            ILogger log)
        {
            log.LogInformation($"Get call for ID {substr}");
                        
            if(req.Method == "GET")
            {
                return await ProcessGet(substr);
            }
            return await ProcessPost(req, substr, log);                      
        }

        private async Task<IActionResult> ProcessPost(HttpRequest req, string substr, ILogger log)
        {
            string price = req.Query["price"];
            Subscription subscription = new Subscription();
            subscription.Id = substr;
            subscription.PartitionKey = "default";
            subscription.Price = Int32.Parse(price);
            log.LogInformation($"Id: {subscription.Id} price: {subscription.Price}.");
            await _adapter.Save<Subscription>(subscription);

            string responseMessage = $"POST request id: {substr}, price: {price}.";
            return new OkObjectResult(responseMessage);

        }

        private async Task<IActionResult> ProcessGet(string substr)
        {
            var subscribtion = await _adapter.Load<Subscription>(substr);

            string responseMessage = $"Subscription ID: {substr}; Cost: {subscribtion.Price}.";

            return new OkObjectResult(responseMessage);
        }
    }
}
