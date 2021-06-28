using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class OrderFunction
    {
        [FunctionName("OrderFunction")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            Binder binder,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            
            log.LogInformation(requestBody);

            dynamic data = JsonConvert.DeserializeObject(requestBody);

            string order = data.data;
            var attributes = new Attribute[]
            {
	            new BlobAttribute($"orders/{data.id}.json", FileAccess.Write),
	            new StorageAccountAttribute("AzureWebJobsStorage")
            };

            using (var writer = await binder.BindAsync<TextWriter>(attributes))
            {
	            await writer.WriteLineAsync(order);
            }

            return new OkObjectResult(order);
        }
    }
}
