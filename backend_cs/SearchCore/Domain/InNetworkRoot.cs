using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchCore.Domain;

// these classes model the in-network file source
// and are also used in output.. we might want to create separate classes for output

public class InNetworkRoot
{
	[JsonPropertyName("provider_references")]
	public List<ProviderReference>? ProviderReferences { get; set; }

	[JsonPropertyName("in_network")]
	public List<Procedure>? InNetworks { get; set; }
}

public class Provider
{
	[JsonPropertyName("npi")]
	public List<long>? Npi { get; set; }

	[JsonPropertyName("tin")]
	public Tin? Tin { get; set; }
}

public class Tin
{
	[JsonPropertyName("type")]
	public string? Type { get; set; }

	[JsonPropertyName("value")]
	public string? Value { get; set; }

	[JsonPropertyName("business_name")]
	public string? BusinessName { get; set; }
}

public class ProviderReference // seems like this more what I would consider a ProviderGroup
{
	[JsonPropertyName("provider_group_id")]
	public int ProviderGroupId { get; set; } // valid only within the file

	[JsonPropertyName("provider_groups")]
	public List<Provider>? Providers { get; set; }
}

public class Procedure // in the file, in_network is a list of these
{
	[JsonPropertyName("name")]
	public string? Name { get; set; } // e.g. acne surgery

	[JsonPropertyName("billing_code")]
	public string? BillingCode { get; set; }

	[JsonPropertyName("negotiated_rates")]
	public List<NegotiatedRate>? NegotiatedRates { get; set; } // this here is why we need separate input and output classes -- we don't want this in the output
}

public class NegotiatedRate
{
	[JsonPropertyName("negotiated_prices")]
	public List<NegotiatedPrice>? NegotiatedPrices { get; set; }

	[JsonPropertyName("provider_references")]
	public List<int>? ProviderReferences { get; set; }
}

public class NegotiatedPrice
{
	[JsonPropertyName("negotiated_rate")]
	public decimal? NegotiatedRate { get; set; }
}
