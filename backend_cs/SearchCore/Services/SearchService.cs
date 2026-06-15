using SearchCore.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SearchCore.Services;

public class SearchService(ICmsTocService tocService, ICmsRatesService ratesService, string cacheFolder)
{
	private const string IndexFile = "2026-04-28_fidelis_index.json";
	private readonly string _cacheFolder = cacheFolder;

	// feels weird without the underscore
	private readonly ICmsTocService _tocService = tocService;
	private readonly ICmsRatesService _ratesService = ratesService;

	public async Task<ServiceResult<List<ProcedureProviderGroupRates>>> ProcedureProviderGroupRates(SearchRequest request)
	{
		var result = new ServiceResult<List<ProcedureProviderGroupRates>>();
		var indexPath = Path.Combine(_cacheFolder, IndexFile);
		var indexResult = await _tocService.DeserializeIndexFile(indexPath);
		if (!indexResult.Success)
		{
			result.Errors.AddRange(indexResult.Errors);
			return result;
		}

		var rates = new List<ProcedureProviderGroupRates>();
		result.Data = rates;

		var mapper = new DomainMapper();

		foreach (var structure in indexResult?.Data?.ReportingStructure ?? [])
		{
			// here's where if we were filtering by issuer or plan we could skip the ones we're not interested in
			// e.g.:
			//if (string.IsNullOrEmpty(request.Issuer) || !structure.ReportingPlans.Any(p => p.IssuerName == request.Issuer))
			//{
			//	continue;
			//}

			// if we wanted to include issuer, I think I would create a new IssuerProcedureProviderGroupRates class
			// possibly with a list of Issuers as one property, and a ProcedureProviderGroupRates as another

			foreach (var file in structure.InNetworkFiles ?? [])
			{
				ServiceResult<InNetworkRoot> rateResult;

				var uri = new Uri(file.Location!);
				var filename = Cached(uri.Segments.Last());
				if (filename is null)
				{
					rateResult = await _ratesService.DeserializeInNetworkFile(uri);
				}
				else
				{
					rateResult = await _ratesService.DeserializeInNetworkFile(filename);
				}

				if (!rateResult.Success)
				{
					result.Errors.AddRange(rateResult.Errors);
					continue;
				}

				if (rateResult.Data is null)
				{
					result.Errors.Add("InNetworkRoot data was not found. Weird?");
					continue;
				}

				var mapped = mapper.MapInNetworkRoot(rateResult.Data, request.BillCode);
				rates.AddRange(mapped);
			}
		}

		return result;
	}

	private string? Cached(string filename)
	{
		var path = Path.Combine(_cacheFolder, filename);
		return File.Exists(path) ? path : null;
	}
}
