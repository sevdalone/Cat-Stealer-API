using CatStealer.Models;
using CatStealer.Services;
using Microsoft.AspNetCore.Mvc;

namespace CatStealer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<JobsController> _logger;

    public JobsController(
        IBackgroundJobService backgroundJobService,
        ILogger<JobsController> logger)
    {
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    // GET: api/jobs/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<JobResponse>> GetJobStatus(string id)
    {
        try
        {
            var jobStatus = await _backgroundJobService.CheckJobStatus(id);

            if (jobStatus.Status == "Not found")
                return NotFound($"Job with ID {id} not found");

            return Ok(jobStatus);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting status for job {id}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}