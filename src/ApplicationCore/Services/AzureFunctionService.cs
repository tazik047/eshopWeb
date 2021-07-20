using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
	public class AzureFunctionService : IAzureFunctionService
	{
		private readonly BaseUrlConfiguration _baseUrlConfiguration;
		private readonly ConnectionStringConfiguration _connectionStringConfiguration;
		private readonly IAppLogger<AzureFunctionService> _appLogger;

		public AzureFunctionService(BaseUrlConfiguration baseUrlConfiguration, ConnectionStringConfiguration connectionStringConfiguration, IAppLogger<AzureFunctionService> appLogger)
		{
			_baseUrlConfiguration = baseUrlConfiguration;
			_connectionStringConfiguration = connectionStringConfiguration;
			_appLogger = appLogger;
		}

		public async Task InvokeDeliveryProcessorAsync(Order order)
		{
			if (string.IsNullOrEmpty(_baseUrlConfiguration.DeliveryProcessorFunc))
			{
				_appLogger.LogWarning("DeliveryProcessor AzureFunc URL is not defined");
				return;
			}

			using (var client = new HttpClient())
			{
				var orderItems = order
					.OrderItems
					.Select(p => new
					{
						ItemId = p.ItemOrdered.CatalogItemId,
						Name = p.ItemOrdered.ProductName,
						PerItemPrice = p.UnitPrice,
						ItemsCount = p.Units
					})
					.ToArray();
				var data = new
				{
					Id = order.Id.ToString(),
					ShippingAddress = order.ShipToAddress,
					OrderItems = orderItems,
					FinalPrice = order.Total(),
					OrderDate = order.OrderDate
				};

				await client.PostAsJsonAsync(_baseUrlConfiguration.DeliveryProcessorFunc, data);
			}
		}

		public async Task InvokeOrderReserverAsync(Order order)
		{
			if (string.IsNullOrEmpty(_baseUrlConfiguration.ServiceBusOrderReserverQueueName))
			{
				_appLogger.LogWarning("OrderReserver queue is not defined");
				return;
			}

			var orderDetails = order
				.OrderItems
				.Select(p => new
				{
					itemId = p.ItemOrdered.CatalogItemId,
					quantity = p.Units
				}).ToArray();

			await SendMessageToServiceBus(order.Id, orderDetails.ToJson(), _baseUrlConfiguration.ServiceBusOrderReserverQueueName);
		}

		private async Task SendMessageToServiceBus(int id, string data, string queueName)
		{
			var client = new ServiceBusClient(_connectionStringConfiguration.ServiceBusConnection);
			var sender = client.CreateSender(queueName);

			try
			{
				var messageBody = new
				{
					id,
					data
				}.ToJson();

				await sender.SendMessageAsync(new ServiceBusMessage(messageBody));
			}
			catch (Exception e)
			{
				_appLogger.LogError(e, "Error during sending message to ServiceBus");
				throw;
			}
			finally
			{
				await sender.DisposeAsync();
				await client.DisposeAsync();
			}
		}
	}
}