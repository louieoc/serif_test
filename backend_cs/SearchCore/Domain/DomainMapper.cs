using System;
using System.Collections.Generic;

namespace SearchCore.Domain;

public class DomainMapper
{
	public List<ProcedureProviderGroupRates> MapInNetworkRoot(InNetworkRoot root, string? billCode, string? ein, long? npi)
	{
		if (root?.ProviderReferences is null)
		{
			throw new Exception("No provider_references found in file.");
		}

		if (root?.InNetworks is null)
		{
			throw new Exception("No in_networks found in file.");
		}

		// map provider group id to our custom rates object
		var groupIdRatesHash = new Dictionary<int, List<ProviderGroupRate>>();
		var output = new List<ProcedureProviderGroupRates>();

		// if npi and ein filters are supplied, create a set of allowed ids
		var allowedProviderIds = GetProviderIdFilter(root.ProviderReferences, ein, npi);

		// first filter providers by billing code
		foreach (var inNetwork in root.InNetworks!)
		{
			// optionally filter by bill code
			if (!string.IsNullOrWhiteSpace(billCode) && inNetwork.BillingCode != billCode) continue;

			if (inNetwork.NegotiatedRates is null) continue; // note: in theory there could be a bill code without any rates, if you want to detect that then don't continue

			var procedure = new Procedure
			{
				BillingCode = inNetwork.BillingCode,
				Name = inNetwork.Name
			};

			var ppgr = new ProcedureProviderGroupRates
			{
				Procedure = procedure
			};

			bool addProcedure = false;
			foreach (var rate in inNetwork.NegotiatedRates)
			{
				if (rate.ProviderReferences is null) continue;
				if (rate.NegotiatedPrices is null) continue;

				foreach (var pr in rate.ProviderReferences)
				{
					// apply ein and npi filter
					if (allowedProviderIds is not null && !allowedProviderIds.Contains(pr)) continue;
					addProcedure = true;

					if (!groupIdRatesHash.TryGetValue(pr, out List<ProviderGroupRate>? groupRates) || groupRates == null)
					{
						groupRates = new List<ProviderGroupRate>();
						groupIdRatesHash[pr] = groupRates;
					}

					var prgr = new ProviderGroupRate
					{
						NegotiatedPrices = rate.NegotiatedPrices
					};

					groupRates.Add(prgr); // this is so we can attach the provider later
					ppgr.GroupRates.Add(prgr);
				}
			}

			// only want to add the procedure if any of the providers match the filters
			if (addProcedure)
			{
				output.Add(ppgr);
			}
		}

		// then fill out the provider info
		foreach (var reference in root.ProviderReferences ?? [])
		{
			if (!groupIdRatesHash.TryGetValue(reference.ProviderGroupId, out List<ProviderGroupRate>? groupRates) || groupRates == null)
			{
				continue;
			}

			foreach (var gr in groupRates)
			{
				foreach (var provider in reference.Providers ?? [])
				{
					// need to filter by ein/npi here too
					if (ProviderIsAllowed(provider, ein, npi))
					{
						gr.Providers.Add(provider);
					}
				}
			}
		}

		return output;
	}

	private HashSet<int>? GetProviderIdFilter(List<ProviderReference> providerRefs, string? ein, long? npi)
	{
		if (string.IsNullOrWhiteSpace(ein) && !npi.HasValue)
		{
			return null;
		}

		var allowed = new HashSet<int>();
		foreach (var providerGroup in providerRefs)
		{
			foreach (var provider in providerGroup.Providers ?? [])
			{
				if (ProviderIsAllowed(provider, ein, npi))
				{
					allowed.Add(providerGroup.ProviderGroupId);
				}
			}
		}
		return allowed;
	}

	public static bool ProviderIsAllowed(Provider provider, string? ein, long? npi)
	{
		if (string.IsNullOrEmpty(ein) && !npi.HasValue)
		{
			return true;
		}

		if (!string.IsNullOrWhiteSpace(ein) && provider.Tin is not null && provider.Tin.Value == ein)
		{
			return true;
		}

		if (npi.HasValue && provider.Npi is not null && provider.Npi.Contains(npi.Value))
		{
			return true;
		}

		return false;
	}
}
