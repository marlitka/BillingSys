using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof(FunctionApp1.Startup))]

namespace FunctionApp1
{   
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            IDbAdapter adp = new DbAdapter(new CosmosClient(System.Environment.GetEnvironmentVariable("CosmosConStr")), "Training");
            builder.Services.AddSingleton((s) => 
            {
                return (adp);
            });            
        }
    }
}