using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace SearchCore.Domain;

// these classes model the Table of Contents / "index" file source

public class CmsIndex
{
	[JsonPropertyName("reporting_entity_name")]
	public string? ReportingEntityName { get; set; }

	[JsonPropertyName("last_updated_on")]
	public string? LastUpdatedOn { get; set; } // ISO date, e.g. "2026-04-28"

	[JsonPropertyName("reporting_structure")]
	public List<ReportingStructure>? ReportingStructure { get; set; }
}

public class ReportingStructure
{
	[JsonPropertyName("reporting_plans")]
	public List<ReportingPlan>? ReportingPlans { get; set; }

	[JsonPropertyName("in_network_files")]
	public List<FileLocation>? InNetworkFiles { get; set; }

	// not sure if we need this
	[JsonPropertyName("allowed_amount_file")]
	public FileLocation? AllowedAmountFile { get; set; }
}

public class ReportingPlan
{
	[JsonPropertyName("plan_name")]
	public string? PlanName { get; set; }

	[JsonPropertyName("plan_id_type")]
	public string? PlanIdType { get; set; } // e.g. "hios" or "ein"

	[JsonPropertyName("plan_id")]
	public string? PlanId { get; set; }

	[JsonPropertyName("plan_market_type")]
	public string? PlanMarketType { get; set; } // e.g. "individual" or "group"

	[JsonPropertyName("issuer_name")]
	public string? IssuerName { get; set; }
}

public class FileLocation
{
	[JsonPropertyName("description")]
	public string? Description { get; set; }

	[JsonPropertyName("location")]
	public string? Location { get; set; }
}
