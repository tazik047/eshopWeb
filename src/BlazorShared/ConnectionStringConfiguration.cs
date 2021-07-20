namespace BlazorShared
{
	public class ConnectionStringConfiguration
	{
		public const string CONFIG_NAME = "ConnectionStrings";

		public string CatalogConnection { get; set; }
		
		public string IdentityConnection { get; set; }
		
		public string ServiceBusConnection { get; set; }
	}
}