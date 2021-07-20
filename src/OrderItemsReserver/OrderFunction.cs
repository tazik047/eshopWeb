using System;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace OrderItemsReserver
{
    public static class OrderFunction
    {
        [FunctionName("OrderFunction")]
        public static async Task Run([ServiceBusTrigger("%MessageQueueName%", Connection = "ServiceBusConnection")]string myQueueItem,
			Binder binder, 
			ILogger log,
			ExecutionContext context)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {myQueueItem}");

			var configurationBuilder = new ConfigurationBuilder()
				.SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			dynamic data = JsonConvert.DeserializeObject(myQueueItem);

			var action = (Func<dynamic, Task>)(async d =>
			{
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
			});
			await RetryService(action, data, log, configurationBuilder);
		}

		private static async Task RetryService(Func<dynamic, Task> action, dynamic data, ILogger logger, IConfiguration configuration)
		{
			var triesCount = 0;
			while (true)
			{
				try
				{
					await action(data);
					return;
				}
				catch (Exception e)
				{
					triesCount++;
					logger.LogError(e, $"Retry error #{triesCount}");

					if (triesCount == 3)
					{
						await SendEmail(data, configuration);
						break;
					}
				}
			}
		}

		private static async Task SendEmail(dynamic data, IConfiguration configuration)
		{
			using (var client = new HttpClient())
			{
				var body = new
				{
					id = data.id,
					data = data.data
				};
				await client.PostAsJsonAsync(configuration["logicAppUrl"], body);
			}
		}
	}
}
