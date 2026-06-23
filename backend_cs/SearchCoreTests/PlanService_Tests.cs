using SearchCore.Services;

namespace SearchCoreTests;

[TestClass]
[TestCategory("Integration")]
public class PlanService_Tests
{
	private const string CacheFolder = "..\\..\\..\\..\\test_files";
	private readonly PlanService _service;

	public PlanService_Tests()
	{
		var client = new HttpClient();
		_service = new PlanService(
			new CmsTocFileService(client),
			CacheFolder
		);
	}

	[TestMethod]
	public async Task GetIssuers_ReturnsIssuers()
	{
		var result = await _service.GetIssuers();

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Count > 0);
		Assert.AreEqual("Fidelis Care", result.Data[0].Name);
	}

	[TestMethod]
	public async Task GetPlans_GivenIssuer_ReturnsPlans()
	{
		var result = await _service.GetPlans("Fidelis Care");

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.Count > 0);
		Assert.AreEqual("Ambetter from Fidelis Care Bronze", result.Data[0].PlanName);
	}
}