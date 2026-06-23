using SearchCore.Services;

namespace SearchCoreTests;

[TestClass]
[TestCategory("Integration")]
public class CmsRatesService_Tests
{
	private const string InNetworkFile = "..\\..\\..\\..\\test_files\\2026-04-28_centene-management-company-llc_fidelis-ex_in-network.json";
	private const string JsonUrl = "https://www.centene.com/content/dam/centene/Centene%20Corporate/json/DOCUMENT/2026-04-28_centene-management-company-llc_fidelis-ex_in-network.json";

	private ICmsRatesService? _service;
	private IPlanService _planService;

	[TestMethod]
	public async Task DeserializeInNetworkFile_GivenUrl_CanDeserialize()
	{
		var httpClient = new HttpClient();
		_service = new CmsRatesService(httpClient);
		var result = await _service.DeserializeInNetworkFile(new Uri(JsonUrl));

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.InNetworks!.Count > 0);
	}

	[TestMethod]
	public async Task DeserializeInNetworkFile_GivenFilename_CanDeserialize()
	{
		_service = new CmsRatesService(null);
		var result = await _service.DeserializeInNetworkFile(InNetworkFile);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.InNetworks!.Count > 0);
	}
}
