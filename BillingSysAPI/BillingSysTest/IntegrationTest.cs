using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FunctionApp1;
using FunctionApp1.Models;
using Microsoft.Azure.Cosmos;

namespace FunctionApp1Test
{
    public class BasicDbAdapterTests : IDisposable
    {
        protected const string CosmosConStr = "CosmosConStr";
       
        public sealed class IgnoreIfNotConnectionFact : FactAttribute
        {
            public IgnoreIfNotConnectionFact()
            {
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(CosmosConStr))) 
                {
                    Skip = "Ignore if no Settings Defined In Environment";
                }
            }
        }

        private readonly CosmosClient _cosmosClient;
        private readonly DbAdapter _adp;
        private readonly string _dbName;

        public BasicDbAdapterTests()
        {
            string? conStr = Environment.GetEnvironmentVariable(CosmosConStr);
            
            if (string.IsNullOrWhiteSpace(conStr)) throw new ArgumentNullException(nameof(conStr));
            

            _cosmosClient = new CosmosClient(conStr);
            
            _dbName = $"test_DbAdapter_{Guid.NewGuid().ToString().Replace("-", "")}";
            _adp = new DbAdapter(_cosmosClient, _dbName);
        }

        public void Dispose()
        {
            Database database = _cosmosClient.GetDatabase(_dbName);
            
            database.DeleteAsync().Wait();
        }

        [IgnoreIfNotConnectionFact]
        public async Task TestCreateSub()
        {
            Subscription subscription = new()
            {
                Id = "10",
                PartitionKey = "default",
                Price = 100
            };

            await _adp.Save(subscription);
            Subscription sub = await _adp.Load<Subscription>(subscription.Id);
            Assert.NotNull(sub);
            Assert.Equal(subscription.Id, sub.Id);
            Assert.True(sub.PartitionKey.Equals(subscription.PartitionKey));
        }

        [IgnoreIfNotConnectionFact]
        public async Task TestCreateUser()
        {
            Users user = new()
            {
                Id = "10",
                PartitionKey = "default",
                StartDate = new DateTime(2023, 1, 1)
            };

            await _adp.Save(user);
            Users user1 = await _adp.Load<Users>(user.Id);
            Assert.NotNull(user1);
            Assert.Equal(user.Id, user1.Id);
            Assert.True(user1.PartitionKey.Equals(user.PartitionKey));
        }

        [Fact]
        public async Task TestGetUsersWithSub()
        {
            Users user1 = new()
            {
                Id = "11",
                SubId = "10",
                PartitionKey = "default",
                StartDate = new DateTime(2023, 1, 1)
            };
            await _adp.Save(user1);

            Users user2 = new()
            {
                Id = "12",
                SubId = "10",
                PartitionKey = "default",
                StartDate = new DateTime(2023, 1, 1)
            };
            await _adp.Save(user2);

            Users user3 = new()
            {
                Id = "13",
                SubId = "20",
                PartitionKey = "default",
                StartDate = new DateTime(2023, 1, 1)
            };

            await _adp.Save(user3);

            List<Users> users = await _adp.GetAllUsers("10");

            Assert.Equal(2, users.Count);
        }

    }
}

