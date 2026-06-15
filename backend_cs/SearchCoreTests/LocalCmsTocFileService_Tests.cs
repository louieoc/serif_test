using SearchCore.Services;

namespace SearchCoreTests;

// these integration tests might not pass in all environments, they're more like developer poc testing

[TestClass]
[TestCategory("Integration")]
public class LocalCmsTocFileService_Tests
{
	private const string IndexFile = "..\\..\\..\\..\\test_files\\2026-04-28_fidelis_index.json";

	private readonly ICmsTocService _service = new CmsTocFileService(null);

	[TestMethod]
	public async Task DeserializeIndexFile_CanDeserialize()
	{
		var result = await _service.DeserializeIndexFile(IndexFile);

		Assert.IsTrue(result.Success);
		Assert.IsNotNull(result.Data);
		Assert.IsTrue(result.Data.ReportingStructure!.Count > 0);
	}
}
