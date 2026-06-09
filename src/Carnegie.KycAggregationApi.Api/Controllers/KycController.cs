using Carnegie.KycAggregationApi.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Carnegie.KycAggregationApi.Api.Controllers;

/// <summary>
/// Get aggregated KYC data
/// </summary>
[ApiController]
[Route("kyc-data")]
[Tags("KYC Aggregation")]
public class KycController : ControllerBase
{
    private readonly IKycAggregationService _service;

    public KycController(IKycAggregationService service)
    {
        _service = service;
    }


    /// <summary>
    /// Get aggregated KYC data
    /// </summary>
    /// <param name="ssn">Social Security Number of the customer</param>
    /// <returns></returns>
    [HttpGet("{ssn}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetAggregatedKycData(string ssn)
    {
        var result = await _service.GetAsync(ssn, HttpContext.RequestAborted);
        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}
