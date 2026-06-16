using SearchCore.Domain;

namespace SearchCoreTests;

[TestClass]
public class DomainMapper_Tests
{
	[TestMethod]
	public void ProviderIsAllowed_GivenEinFilter_WorksAsExpected()
	{
		var provider = new Provider
		{
			Npi = new List<long> { 1111111111, 1111111112, 111111113 },
			Tin = new Tin
			{
				Value = "12-3456789"
			}
		};

		Assert.IsTrue(DomainMapper.ProviderIsAllowed(provider, "12-3456789", null));
		Assert.IsFalse(DomainMapper.ProviderIsAllowed(provider, "22-3456789", null));
	}

	[TestMethod]
	public void ProviderIsAllowed_GivenNpiFilter_WorksAsExpected()
	{
		var provider = new Provider
		{
			Npi = new List<long> { 1111111111, 1111111112, 111111113 },
			Tin = new Tin
			{
				Value = "12-3456789"
			}
		};

		Assert.IsTrue(DomainMapper.ProviderIsAllowed(provider, "12-3456789", 1111111113));
		Assert.IsFalse(DomainMapper.ProviderIsAllowed(provider, "22-3456789", 4));
	}

	[TestMethod]
	public void ProviderIsAllowed_GivenBothFilters_WorksAsExpected()
	{
		var provider = new Provider
		{
			Npi = new List<long> { 1111111111, 1111111112, 111111113 },
			Tin = new Tin
			{
				Value = "12-3456789"
			}
		};

		Assert.IsTrue(DomainMapper.ProviderIsAllowed(provider, "12-3456789", 2222222222));
		Assert.IsTrue(DomainMapper.ProviderIsAllowed(provider, "22-3456789", 1111111112));
		Assert.IsFalse(DomainMapper.ProviderIsAllowed(provider, "22-3456789", 2111111112));
	}

	[TestMethod]
	public void ProviderIsAllowed_GivenNoFilters_ReturnsTrue()
	{
		var provider = new Provider
		{
			Npi = new List<long> { 1111111111, 1111111112, 111111113 },
			Tin = new Tin
			{
				Value = "12-3456789"
			}
		};

		Assert.IsTrue(DomainMapper.ProviderIsAllowed(provider, null, null));
	}
}