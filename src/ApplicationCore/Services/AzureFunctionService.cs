using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using BlazorShared;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using Microsoft.eShopWeb.ApplicationCore.Interfaces;

namespace Microsoft.eShopWeb.ApplicationCore.Services
{
	public class AzureFunctionService : IAzureFunctionService
	{
		private readonly BaseUrlConfiguration _baseUrlConfiguration;
		private readonly IAppLogger<AzureFunctionService> _appLogger;

		public AzureFunctionService(BaseUrlConfiguration baseUrlConfiguration, IAppLogger<AzureFunctionService> appLogger)
		{
			_baseUrlConfiguration = baseUrlConfiguration;
			_appLogger = appLogger;
		}

		public async Task InvokeOrderReserverAsync(Order order)
		{
			if (string.IsNullOrEmpty(_baseUrlConfiguration.OrderReserverFunc))
			{
				_appLogger.LogWarning("OrderReserver AzureFunc URL is not defined");
			}

			using (var client = new HttpClient())
			{
				var orderDetails = order
					.OrderItems
					.Select(p => new
					{
						itemId = p.ItemOrdered.CatalogItemId,
						quantity = p.Units
					}).ToArray();

				var data = new
				{
					Id = order.Id,
					Data = orderDetails.ToJson()
				};

				await client.PostAsJsonAsync(_baseUrlConfiguration.OrderReserverFunc, data);
			}
		}
	}
}