using SearchCore.Domain;
using System;
using System.Threading.Tasks;

namespace SearchCore.Services
{
	public interface ICmsTocService
	{
		Task<ServiceResult<CmsIndex>> DeserializeIndexFile(Uri uri, string? cacheFolder = null);
		Task<ServiceResult<CmsIndex>> DeserializeIndexFile(string filename);
	}
}
