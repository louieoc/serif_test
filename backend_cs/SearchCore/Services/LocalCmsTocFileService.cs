using SearchCore.Domain;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace SearchCore.Services
{
	public class LocalCmsTocFileService : ICmsTocService
	{
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
