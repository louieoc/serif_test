using SearchCore.Domain;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SearchCore.Services
{
	public interface IPlanService
	{
		Task<ServiceResult<List<Issuer>>> GetIssuers();
		Task<ServiceResult<List<ReportingPlan>>> GetPlans(string issuer);
	}
}
