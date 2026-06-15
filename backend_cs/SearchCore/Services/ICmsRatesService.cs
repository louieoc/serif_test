using SearchCore.Domain;
using System;
using System.Threading.Tasks;

namespace SearchCore.Services;


public interface ICmsRatesService
{
	Task<ServiceResult<InNetworkRoot>> DeserializeInNetworkFile(Uri uri);
	Task<ServiceResult<InNetworkRoot>> DeserializeInNetworkFile(string filename);
}
