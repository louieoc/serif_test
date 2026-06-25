using SearchCore.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SearchCore.Services;

public class SearchService(ICmsTocService tocService, ICmsRatesService ratesService, string cacheFolder)
{
	private const string IndexUrl = "https://www.centene.com/content/dam/centene/Centene%20Corporate/json/DOCUMENT/2026-04-28_fidelis_index.json";
	private readonly string _cacheFolder = cacheFolder;

	// feels weird without the underscore
	private readonly ICmsTocService _tocService = tocService;
	private readonly ICmsRatesService _ratesService = ratesService;

	public async Task<ServiceResult<List<ProcedureProviderGroupRates>>> ProcedureProviderGroupRates(SearchRequest request)
	{
		var output = new ServiceResult<List<ProcedureProviderGroupRates>>();

		var indexResult = new ServiceResult<CmsIndex>();
		var indexUri = new Uri(IndexUrl);
		var indexFile = Cached(indexUri.Segments.Last());
		if (indexFile is null)
		{
			indexResult = await _tocService.DeserializeIndexFile(indexUri, _cacheFolder);
		}
		else
		{
			indexResult = await _tocService.DeserializeIndexFile(indexFile);
		}

		if (!indexResult.Success)
		{
			output.Errors.AddRange(indexResult.Errors);
			return output;
		}

		if (indexResult.Data is null)
		{
			output.Errors.Add("CMS index data was not found. Weird?");
			return output;
		}

		var rates = new List<ProcedureProviderGroupRates>();
		output.Data = rates;

		var mapper = new DomainMapper();

		foreach (var structure in indexResult?.Data?.ReportingStructure ?? [])
		{
			// filter by issuer
			if (!string.IsNullOrEmpty(request.IssuerName) && !structure.ReportingPlans!.Any(p => p.IssuerName == request.IssuerName))
			{
				continue;
			}

			// filter by plan
			var plans = new List<ReportingPlan>();
			bool planPresent = string.IsNullOrEmpty(request.Plan) || false;
			if (!planPresent)
			{
				foreach (var plan in structure.ReportingPlans ?? [])
				{
					if (plan.PlanName!.Equals(request.Plan, StringComparison.OrdinalIgnoreCase))
					{
						planPresent = true;
					}
				}
			}
			if (!planPresent)
			{
				continue;
			}
			else
			{
				plans.AddRange(structure.ReportingPlans ?? []);
			}

			foreach (var file in structure.InNetworkFiles ?? [])
			{
				ServiceResult<InNetworkRoot> rateResult;

				var uri = new Uri(file.Location!);
				var rateFile = Cached(uri.Segments.Last());
				if (rateFile is null)
				{
					rateResult = await _ratesService.DeserializeInNetworkFile(uri, _cacheFolder);
				}
				else
				{
					rateResult = await _ratesService.DeserializeInNetworkFile(rateFile);
				}

				if (!rateResult.Success)
				{
					output.Errors.AddRange(rateResult.Errors);
					continue;
				}

				if (rateResult.Data is null)
				{
					output.Errors.Add("InNetworkRoot data was not found. Weird?");
					continue;
				}

				long? longNpi = ParseLong(request.Npi);
				var mapped = mapper.MapInNetworkRoot(rateResult.Data, request.BillCode, request.Ein, longNpi, request.IssuerName, plans);
				rates.AddRange(mapped);
			}
		}

		return output;
	}

	private string? Cached(string filename)
	{
		var path = Path.Combine(_cacheFolder, filename);
		return File.Exists(path) ? path : null;
	}

	private static long? ParseLong(string? myLong)
	{
		if (long.TryParse(myLong, out long npiLong))
		{
			return npiLong;
		}
		return null;
	}
}
