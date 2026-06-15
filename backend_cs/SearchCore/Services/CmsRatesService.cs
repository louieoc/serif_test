using SearchCore.Domain;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace SearchCore.Services;

public class CmsRatesService(HttpClient? httpClient) : ICmsRatesService
{
	private readonly HttpClient? _httpClient = httpClient;

	public async Task<ServiceResult<InNetworkRoot>> DeserializeInNetworkFile(Uri uri, string? cacheFolder = null)
	{
		var result = new ServiceResult<InNetworkRoot>();
		if (_httpClient is null)
		{
			result.Errors.Add("HttpClient is null.");
			return result;
		}

		try
		{
			if (_httpClient.DefaultRequestHeaders.UserAgent is null || _httpClient.DefaultRequestHeaders.UserAgent.Count == 0)
			{
				// Centene's CDN returns 403 for requests with no User-Agent header,
				// which HttpClient omits by default.
				_httpClient.DefaultRequestHeaders.UserAgent!.ParseAdd("CmsSearchTest/1.0");
			}

			using var httpResult = await _httpClient.GetAsync(uri);
			httpResult.EnsureSuccessStatusCode();
			using Stream streamToReadFrom = await httpResult.Content.ReadAsStreamAsync();

			if (!string.IsNullOrEmpty(cacheFolder))
			{
				if (!Directory.Exists(cacheFolder))
				{
					Directory.CreateDirectory(cacheFolder);
				}

				var path = Path.Combine(cacheFolder, uri.Segments.Last()); // might be better to input the full path rather than the cache folder alone
				using var fileStream = File.Create(path);
				await streamToReadFrom.CopyToAsync(fileStream);
				streamToReadFrom.Seek(0, SeekOrigin.Begin);
			}

			InNetworkRoot? root = await JsonSerializer.DeserializeAsync<InNetworkRoot>(streamToReadFrom);
			result.Data = root;
		}
		catch
		{
			// ideally would log here and not return anything specific to the user
			result.Errors.Add("Exception while downloading rates file.");
		}

		return result;
	}

	public async Task<ServiceResult<InNetworkRoot>> DeserializeInNetworkFile(string filename)
	{
		var result = new ServiceResult<InNetworkRoot>();
		if (!File.Exists(filename))
		{
			result.Errors.Add("In network rate file not found.");
			return result;
		}

		try
		{
			string json = await File.ReadAllTextAsync(filename);
			InNetworkRoot? root = JsonSerializer.Deserialize<InNetworkRoot>(json);
			result.Data = root;
		}
		catch
		{
			// ideally would log here and not return anything specific to the user
			result.Errors.Add("Exception while parsing rates file.");
		}

		return result;
	}
}
