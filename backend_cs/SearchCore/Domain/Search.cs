using System.Collections.Generic;

namespace SearchCore.Domain;

/// <summary>
/// output:
/// link a group of providers that share the same prices
/// </summary>
public class ProviderGroupRate
{
	public List<Provider> Providers { get; set; } = [];
	public List<NegotiatedPrice> NegotiatedPrices { get; set; } = [];
}

/// <summary>
///  Not sure what the name should be, but provider_group_id is the key that combines
///  a procedure with groups of providers sharing a set of prices
/// </summary>
public class ProcedureProviderGroupRates
{
	public Procedure? Procedure { get; set; }
	public List<ProviderGroupRate> GroupRates { get; set; } = [];
}
