using SearchCore.Domain;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System;
using System.Net.Http;
using System.Linq;

namespace SearchCore.Services
{
	public class CmsTocFileService(HttpClient? httpClient) : ICmsTocService
	{
		private HttpClient? _httpClient = httpClient;

		public async Task<ServiceResult<CmsIndex>> DeserializeIndexFile(Uri uri, string? cacheFolder = null)
		{
			var result = new ServiceResult<CmsIndex>();
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

					var path = Path.Combine(cacheFolder, uri.Segments.Last());
					using var fileStream = File.Create(path);
					await streamToReadFrom.CopyToAsync(fileStream);
					streamToReadFrom.Seek(0, SeekOrigin.Begin);
				}

				CmsIndex? root = await JsonSerializer.DeserializeAsync<CmsIndex>(streamToReadFrom);
				result.Data = root;
			}
			catch
			{
				// ideally would log here and not return anything specific to the user
				result.Errors.Add("Exception while downloading index file.");
			}

			return result;
		}

		public async Task<ServiceResult<CmsIndex>> DeserializeIndexFile(string filename)
		{
			var result = new ServiceResult<CmsIndex>();

			if (!File.Exists(filename))
			{
				result.Errors.Add("File not found.");
				return result;
			}

			string json = await File.ReadAllTextAsync(filename);
			CmsIndex? root = JsonSerializer.Deserialize<CmsIndex>(json);
			result.Data = root;
			return result;
		}
	}
}
