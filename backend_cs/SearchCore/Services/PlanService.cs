using SearchCore.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SearchCore.Services;

public class PlanService(ICmsTocService tocService, string cacheFolder) : IPlanService
{
	private const string IndexUrl = "https://www.centene.com/content/dam/centene/Centene%20Corporate/json/DOCUMENT/2026-04-28_fidelis_index.json";
	private readonly string _cacheFolder = cacheFolder;

	private readonly ICmsTocService _tocService = tocService;

	public async Task<ServiceResult<List<Issuer>>> GetIssuers()
	{
		var output = new ServiceResult<List<Issuer>>();

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

		var plans = indexResult.Data!.ReportingStructure!.SelectMany(rs => rs.ReportingPlans!).ToList();
		var issuerNames = plans.Select(p => p.IssuerName).Distinct();
		var issuers = issuerNames.Select(n => new Issuer
		{
			Name = n
		});

		output.Data = issuers.ToList();
		return output;
	}

	public async Task<ServiceResult<List<ReportingPlan>>> GetPlans(string issuer)
	{
		var output = new ServiceResult<List<ReportingPlan>>();

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

		var plans = indexResult.Data!.ReportingStructure!.SelectMany(rs => rs.ReportingPlans!).ToList();
		output.Data = plans.ToList();
		return output;
	}


	private string? Cached(string filename)
	{
		var path = Path.Combine(_cacheFolder, filename);
		return File.Exists(path) ? path : null;
	}
}
