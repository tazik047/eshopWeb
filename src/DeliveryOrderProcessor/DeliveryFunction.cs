using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace DeliveryOrderProcessor
{
    public static class DeliveryFunction
    {
        [FunctionName("DeliveryFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", "post", Route = null)] HttpRequest req,
            [CosmosDB(
	            databaseName: "delivery-processor",
	            collectionName: "orders",
	            ConnectionStringSetting = "CosmosDbConnectionString")]IAsyncCollector<string> documentsOut,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

            log.LogInformation($"Request body: {requestBody}");

            await documentsOut.AddAsync(requestBody);

            return new OkObjectResult(requestBody);
        }
    }
}
