using System.Threading.Tasks;
using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces
{
	public interface IAzureFunctionService
	{
		Task InvokeOrderReserverAsync(Order order);
	}
}