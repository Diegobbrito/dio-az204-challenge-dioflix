using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

namespace fnGetMovies
{
    public class FunctionAll
    {
        private readonly ILogger<FunctionAll> _logger;
        private readonly CosmosClient _cosmosClient;

        public FunctionAll(ILogger<FunctionAll> logger, CosmosClient cosmosClient)
        {
            _logger = logger;
            _cosmosClient = cosmosClient;
        }

        [Function("getAll")]
        public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get")] HttpRequestData req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            var container = _cosmosClient.GetContainer("FlixDB", "movies");
            var sqlQueryText = $"SELECT * FROM c";
            var queryDefinition = new QueryDefinition(sqlQueryText);
            var result = container.GetItemQueryIterator<MovieResult>(queryDefinition);
            var results = new List<MovieResult>();
            while (result.HasMoreResults)
            {
                foreach (var item in await result.ReadNextAsync())
                {
                    results.Add(item);
                }
            }
            var responseMessage = req.CreateResponse(System.Net.HttpStatusCode.OK);
            await responseMessage.WriteAsJsonAsync(results);
            return responseMessage;
        }
    }
}
