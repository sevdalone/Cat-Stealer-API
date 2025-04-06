using CatStealer.Data;
using CatStealer.Models;
using CatStealer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CatStealer.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatsController : ControllerBase
{
    private readonly CatDbContext _dbContext;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<CatsController> _logger;

    public CatsController(
        CatDbContext dbContext,
        IBackgroundJobService backgroundJobService,
        ILogger<CatsController> logger)
    {
        _dbContext = dbContext;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    // GET: api/cats
    [HttpGet]
    public async Task<ActionResult<PagedResponse<CatDto>>> GetCats(
        [FromQuery] string tag = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
            return BadRequest("Page must be greater than or equal to 1");

        if (pageSize < 1 || pageSize > 100)
            return BadRequest("Page size must be between 1 and 100");

        try
        {
            IQueryable<CatEntity> query = _dbContext.Cats;

            // Filter by tag if provided
            if (!string.IsNullOrEmpty(tag))
            {
                query = query
                    .Include(c => c.CatTags)
                    .ThenInclude(ct => ct.Tag)
                    .Where(c => c.CatTags.Any(ct => ct.Tag.Name == tag));
            }

            // Get total count for pagination
            var totalCount = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

            // Apply pagination
            var cats = await query
                .OrderByDescending(c => c.Created)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // Ensure we have tags loaded
            if (string.IsNullOrEmpty(tag))
            {
                foreach (var cat in cats)
                {
                    await _dbContext.Entry(cat)
                        .Collection(c => c.CatTags)
                        .LoadAsync();

                    foreach (var catTag in cat.CatTags)
                    {
                        await _dbContext.Entry(catTag)
                            .Reference(ct => ct.Tag)
                            .LoadAsync();
                    }
                }
            }

            // Map to DTOs
            var catDtos = cats.Select(c => new CatDto
            {
                Id = c.Id,
                CatId = c.CatId,
                Width = c.Width,
                Height = c.Height,
                ImageUrl = Url.Action("GetCatImage", "Cats", new { id = c.Id }, Request.Scheme),
                Created = c.Created,
                Tags = c.CatTags.Select(ct => ct.Tag.Name).ToList()
            }).ToList();

            return Ok(new PagedResponse<CatDto>
            {
                Items = catDtos,
                PageNumber = page,
                PageSize = pageSize,
                TotalPages = totalPages,
                TotalCount = totalCount
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while getting cats");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    // GET: api/cats/{id}
    [HttpGet("{id}")]
    public async Task<ActionResult<CatDto>> GetCat(int id)
    {
        try
        {
            var cat = await _dbContext.Cats
                .Include(c => c.CatTags)
                .ThenInclude(ct => ct.Tag)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cat == null)
                return NotFound($"Cat with ID {id} not found");

            var catDto = new CatDto
            {
                Id = cat.Id,
                CatId = cat.CatId,
                Width = cat.Width,
                Height = cat.Height,
                ImageUrl = Url.Action("GetCatImage", "Cats", new { id = cat.Id }, Request.Scheme),
                Created = cat.Created,
                Tags = cat.CatTags.Select(ct => ct.Tag.Name).ToList()
            };

            return Ok(catDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting cat with ID {id}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    // GET: api/cats/{id}/image
    [HttpGet("{id}/image")]
    public async Task<IActionResult> GetCatImage(int id)
    {
        try
        {
            var cat = await _dbContext.Cats.FindAsync(id);

            if (cat == null)
                return NotFound($"Cat with ID {id} not found");

            return File(cat.Image, "image/jpeg");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"An error occurred while getting image for cat with ID {id}");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }

    // POST: api/cats/fetch
    [HttpPost("fetch")]
    public IActionResult FetchCats()
    {
        try
        {
            var jobId = _backgroundJobService.QueueCatsFetch();

            return Ok(new JobResponse
            {
                JobId = jobId,
                Status = "Queued"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred while queuing cat fetch job");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
}