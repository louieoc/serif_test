using System.Collections.Generic;

namespace SearchCore.Domain;

/// <summary>
/// output:
/// link a group of providers that share the same prices
/// </summary>
public class ProviderGroupRate
{
	// should there be an ID for this? Each InNetwork file has an internal sequence that could map to this object
	// otherwise I don't see one. In DomainMapper I use a Dictionary<int, ProviderGroupRate> to track this idea
	public List<Provider> Providers { get; set; } = [];
	public List<NegotiatedPrice> NegotiatedPrices { get; set; } = [];
}

/// <summary>
///  Not sure what the name should be, but provider_group_id is the key that combines
///  a procedure with groups of providers sharing a set of prices
/// </summary>
public class ProcedureProviderGroupRates
{
	public Issuer? Issuer { get; set; }
	public List<ReportingPlan> Plans { get; set; } = [];
	public Procedure? Procedure { get; set; }
	public List<ProviderGroupRate> GroupRates { get; set; } = [];
}

public class Issuer
{
	public string Name { get; set; } = string.Empty;
}
