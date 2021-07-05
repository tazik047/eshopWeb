using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Microsoft.eShopWeb
{
	public static class DbExtensions
	{
		public static async Task MigrateDb(this DbContext context)
		{
			if (!context.Database.IsRelational())
			{
				return;
			}

			await context.Database.MigrateAsync();
		}
	}
}