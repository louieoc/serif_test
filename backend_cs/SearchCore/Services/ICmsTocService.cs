using SearchCore.Domain;
using System.Threading.Tasks;

namespace SearchCore.Services
{
	public interface ICmsTocService
	{
		Task<ServiceResult<CmsIndex>> DeserializeIndexFile(string filename);
	}
}
