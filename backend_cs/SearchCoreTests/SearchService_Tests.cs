using SearchCore.Domain;
using SearchCore.Services;

namespace SearchCoreTests;

[TestClass]
[TestCategory("Integration")]
public class SearchService_Tests
{
	private const string CacheFolder = "..\\..\\..\\..\\test_files";
	private readonly SearchService _service;

	public SearchService_Tests()
	{
		var client = new HttpClient();
		_service = new SearchService(
			new CmsTocFileService(client),
			new CmsRatesService(client),
			CacheFolder
		);
	}
	
	[TestMethod]
	public async Task ProcedureProviderGroupRates_CanSearchByBillCode()
	{
		var request = new SearchRequest
		{
			BillCode = "10040"
		};
		var result = await _service.ProcedureProviderGroupRates(request);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Count > 0);

		WriteToConsole(result.Data);
	}

	private static void WriteToConsole(List<ProcedureProviderGroupRates> rates)
	{
		foreach (var item in rates)
		{
			Console.WriteLine("Procedure:");
			Console.WriteLine($"	Bill code: {item.Procedure?.BillingCode}");
			Console.WriteLine($"	Name: {item.Procedure?.Name}");

			foreach (var rate in item.GroupRates)
			{
				Console.WriteLine("  Providers:");
				foreach (var provider in rate.Providers ?? [])
				{
					Console.WriteLine($"	Name: {provider.Tin?.BusinessName} Ein: {provider?.Tin?.Value}");
				}

				Console.WriteLine("  Prices:");
				foreach (var price in rate.NegotiatedPrices ?? [])
				{
					Console.WriteLine($"	${price.NegotiatedRate}");
				}
			}
		}
	}
}
