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

	[TestMethod]
	public async Task ProcedureProviderGroupRates_GivenIssuerFilter_IncludesIssuer()
	{
		var request = new SearchRequest
		{
			BillCode = "10040",
			IssuerName = "Fidelis Care"
		};
		var result = await _service.ProcedureProviderGroupRates(request);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Count > 0);
		Assert.AreEqual("Fidelis Care", result.Data[0].Issuer!.Name);

		WriteToConsole(result.Data);
	}

	[TestMethod]
	public async Task ProcedureProviderGroupRates_GivenPlanFilter_IncludesPlan()
	{
		var request = new SearchRequest
		{
			BillCode = "99213",
			Plan = "Essential Plan 4"
		};
		var result = await _service.ProcedureProviderGroupRates(request);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Count > 0);
		Assert.IsTrue(result.Data.All(d => d.Plans!.Any(p => p.PlanName == "Essential Plan 4")));

		WriteToConsole(result.Data);
	}

	private static void WriteToConsole(List<ProcedureProviderGroupRates> rates)
	{
		foreach (var item in rates)
		{
			Console.WriteLine("Procedure:");
			Console.WriteLine($"	Bill code: {item.Procedure?.BillingCode}");
			Console.WriteLine($"	Name: {item.Procedure?.Name}");
			Console.WriteLine($"	Issuer: {item.Issuer?.Name ?? string.Empty}");

			Console.WriteLine("  Plans:");
			foreach (var plan in item.Plans ?? [])
			{
				Console.WriteLine($"	Name: {plan.PlanName}");
			}

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
