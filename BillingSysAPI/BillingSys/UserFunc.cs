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
using System.Collections.Generic;

namespace FunctionApp1
{
    public class UserFunc
    {
        private readonly IDbAdapter _adapter;
        public UserFunc(IDbAdapter adp)
        {
            _adapter = adp;
        }

        [FunctionName("SubscriptionUsers")]
        public async Task<IActionResult> RunUsers(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "subscriptionUsers/{subId}/")] HttpRequest req, string subId,
            ILogger log)
        {                       
            return await ProcessGetUsersSub(subId, log);
        }

        

        [FunctionName("User")]
        public async Task<IActionResult> RunUser(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "user/{userId}/")] HttpRequest req, string userId,
            ILogger log)
        {
            if (req.Method == "GET")
            {
                return await ProcessGet(userId, log);
            }
            return await ProcessLoad(req, userId, log);
        }

        private async Task<IActionResult> ProcessGetUsersSub(string subId, ILogger log)
        {
            List<Users> AllUsersSub = await _adapter.GetAllUsers(subId);
            string responseMessage = string.Empty;
            foreach (Users User in AllUsersSub)
            {
                responseMessage += $"User ID: {User.Id}; SubId: {User.SubId} ";
            }
            
            log.LogInformation($"Get call for SubID {subId}");
            return new OkObjectResult(responseMessage);

        }
        private async Task<IActionResult> ProcessGet(string userId, ILogger log)
        {
            var user = await _adapter.Load<Users>(userId);

            string responseMessage = $"User ID: {userId}; SubId: {user.SubId}; StartDate: {user.StartDate};  EndDate: {user.EndDate}.";
            log.LogInformation($"Get call for ID {userId}");
            return new OkObjectResult(responseMessage);
        }

        private async Task<IActionResult> ProcessLoad(HttpRequest req, string userId, ILogger log)
        {
            string subId = req.Query["subId"];
            string startDate = req.Query["startDate"];
            string endDate = req.Query["endDate"];

            Users user = new Users();
            user.Id = userId;
            user.PartitionKey = "default";
            user.StartDate = DateTime.Parse(startDate);    
            user.EndDate = DateTime.Parse(endDate);

            log.LogInformation($"Id: {user.Id}, startDate: {startDate}, endDate {endDate}.");
            await _adapter.Save(user);

            string responseMessage = $"POST request id: {userId}, startDate: {startDate}, endDate {endDate} .";
            return new OkObjectResult(responseMessage);
        }
    }
}
