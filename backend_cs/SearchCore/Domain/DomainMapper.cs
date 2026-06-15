using System;
using System.Collections.Generic;

namespace SearchCore.Domain;

public class DomainMapper
{
	public List<ProcedureProviderGroupRates> MapInNetworkRoot(InNetworkRoot root, string? billCode)
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
			output.Add(ppgr);

			foreach (var rate in inNetwork.NegotiatedRates)
			{
				if (rate.ProviderReferences is null) continue;
				if (rate.NegotiatedPrices is null) continue;

				foreach (var pr in rate.ProviderReferences)
				{
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
				if (gr.Providers is null)
				{
					gr.Providers = new List<Provider>();
				}

				foreach (var provider in reference.Providers ?? [])
				{
					gr.Providers.Add(provider);
				}
			}
		}

		return output;
	}
}
