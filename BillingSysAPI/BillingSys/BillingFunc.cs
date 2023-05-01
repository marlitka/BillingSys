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
    public class BillingFunc
    {
        private readonly IDbAdapter _adapter;
        public BillingFunc(IDbAdapter adp)
        {
            _adapter = adp;
        }        

        [FunctionName("Billing")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = "billing/{subId}/{date}/")] HttpRequest req, string subId, string date,
            ILogger log)
        {
            log.LogInformation($"Get call for ID {subId} and month {date}");
                                    
            double sum = await TotalSum(date, subId);
            string responseMessage = $"POST request id: {subId}, total sum: {sum}.";
            return new OkObjectResult(responseMessage);
        }               
       
        public async ValueTask<double> TotalSum(string date, string subId)
        {
            List<Users> users = await _adapter.GetAllUsers(subId);
            var subscription = await _adapter.Load<Subscription>(subId);
            DateTime dateN = DateTime.Parse(date);

            var firstDayOfMonth = new DateTime(dateN.Year, dateN.Month, 1);
            var lastDayOfMonth = firstDayOfMonth.AddMonths(1).AddSeconds(-1);

            int daysCount = DateTime.DaysInMonth(DateTime.Parse(date).Year, DateTime.Parse(date).Month);
            double sum = 0;

            foreach (var user in users)
            {
                DateTime startDate = user.StartDate > firstDayOfMonth ? user.StartDate : firstDayOfMonth;
                DateTime endDate = user.EndDate < lastDayOfMonth ? user.EndDate : lastDayOfMonth;
                int dd = (int)(endDate - startDate).TotalDays + 1;
                dd = dd < 0 ? 0 : dd;

                sum += dd * subscription.Price / daysCount;
            }            
            return sum;            
        }
    }
}
