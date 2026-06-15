using Microsoft.AspNetCore.Mvc;
using SearchCore.Services;

namespace SearchApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ICmsRatesService _ratesService;
	private readonly ICmsTocService _tocService;
    private readonly ILogger<SearchController> _logger;
	private const string CacheFolder = "..\\test_files";

	public SearchController(
        ICmsRatesService ratesService,
		ICmsTocService tocService,
        ILogger<SearchController> logger)
    {
        _ratesService = ratesService;
		_tocService = tocService;
        _logger = logger;
    }

	[HttpGet("rates")]
	public async Task<IActionResult> GetRates([FromQuery] string billCode, CancellationToken cancellationToken)
	{
		if (string.IsNullOrEmpty(billCode))
		{
			return BadRequest("Please supply a bill code.");
		}

		var service = new SearchService(_tocService, _ratesService, CacheFolder);
		var request = new SearchRequest
		{
			BillCode = billCode
		};
		var serviceResult = await service.ProcedureProviderGroupRates(request);
		if (!serviceResult.Success)
		{
			return StatusCode(StatusCodes.Status500InternalServerError);
		}

		return Ok(serviceResult.Data);
	}
}
