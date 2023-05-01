using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FunctionApp1.Models;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;

namespace FunctionApp1
{
    public interface IDbAdapter
    {
        ValueTask<List<Users>> GetAllUsers(string subId);
        ValueTask<T> Load<T>(string id);
        Task Save<T>(T obj);
    }

    public class DbAdapter : IDbAdapter
    {
        private readonly CosmosClient _cosmosClient;
        private const string PartKey = "default";
        private readonly string _dbName;


        public DbAdapter(CosmosClient cosmosClient, string dbName)
        {
            _cosmosClient = cosmosClient;
            _dbName = dbName;
        }

        private async ValueTask<Container> GetContainer(string name)
        {
            var db = await _cosmosClient.CreateDatabaseIfNotExistsAsync(_dbName);
            var cntr = await db.Database.CreateContainerIfNotExistsAsync(name, "/PartitionKey");

            return cntr.Container;
        }

        public async Task Save<T>(T obj)
        {
            var cntr = await GetContainer(typeof(T).Name);
            await cntr.UpsertItemAsync<T>(obj);
        }

        public async ValueTask<T> Load<T>(string id)
        {
            var cntr = await GetContainer(typeof(T).Name);
            var res = await cntr.ReadItemAsync<T>(id, new PartitionKey(PartKey));
            return res;
        }

        public async ValueTask<List<Users>> GetAllUsers(string subId)
        {
            string typeName = typeof(Users).Name; // returns name of generic type
            List<Users> result = new();
            try
            {
                string sql = $"SELECT * FROM {typeName} A WHERE A.SubId = '{subId}'";
                QueryDefinition query = new QueryDefinition(sql);

                var container = await GetContainer(typeName);
                using (FeedIterator<Users> setIterator = container.GetItemQueryIterator<Users>(query))
                {
                    while (setIterator.HasMoreResults)
                    {
                        FeedResponse<Users> currentSet = await setIterator.ReadNextAsync();
                        currentSet.ToList().ForEach(user => result.Add(user));
                    }
                }
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                // expected exception if no records found
            }
            return result;
        }

    }

}

