using CatStealer.Data;
using CatStealer.Models;
using Hangfire;
using Microsoft.EntityFrameworkCore;

namespace CatStealer.Services;

public interface IBackgroundJobService
    {
        string QueueCatsFetch();
        Task<JobResponse> CheckJobStatus(string jobId);
    }

public class BackgroundJobService : IBackgroundJobService
{
    private readonly ICatApiService _catApiService;
    private readonly ILogger<BackgroundJobService> _logger;
    private readonly IServiceProvider _serviceProvider;

    public BackgroundJobService(ICatApiService catApiService, ILogger<BackgroundJobService> logger,
        IServiceProvider serviceProvider)
    {
        _catApiService = catApiService;
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public string QueueCatsFetch()
    {
        var jobId = BackgroundJob.Enqueue(() => FetchCatsJob());
        return jobId;
    }

    public Task<JobResponse> CheckJobStatus(string jobId)
    {
        var jobDetails = JobStorage.Current.GetConnection().GetJobData(jobId);

        return Task.FromResult(new JobResponse
        {
            JobId = jobId,
            Status = jobDetails?.State ?? "Not found"
        });
    }

    public async Task FetchCatsJob()
    {
        _logger.LogInformation("Starting cat fetching job");

        try
        {
            // Get cats from the API
            var cats = await _catApiService.GetCatsWithBreeds();

            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<CatDbContext>();

            foreach (var cat in cats)
            {
                // Check if the cat already exists in the database
                var existingCat = await dbContext.Cats.FirstOrDefaultAsync(c => c.CatId == cat.Id);
                if (existingCat != null)
                {
                    _logger.LogInformation($"Cat {cat.Id} already exists. Skipping.");
                    continue;
                }

                // Download the image
                var imageBytes = await _catApiService.DownloadImage(cat.Url);

                // Create a new cat entity
                var catEntity = new CatEntity
                {
                    CatId = cat.Id,
                    Width = cat.Width,
                    Height = cat.Height,
                    Image = imageBytes,
                    Created = DateTime.UtcNow
                };

                // Process temperament tags if breeds are available
                if (cat.Breeds != null && cat.Breeds.Any())
                {
                    var breedTemperaments = cat.Breeds
                        .Where(b => !string.IsNullOrEmpty(b.Temperament))
                        .SelectMany(b => b.Temperament.Split(','))
                        .Select(t => t.Trim())
                        .Distinct();

                    foreach (var temperament in breedTemperaments)
                    {
                        // Check if the tag already exists
                        var tagEntity = await dbContext.Tags
                            .FirstOrDefaultAsync(t => t.Name == temperament);

                        if (tagEntity == null)
                        {
                            // Create a new tag
                            tagEntity = new TagEntity
                            {
                                Name = temperament,
                                Created = DateTime.UtcNow
                            };

                            dbContext.Tags.Add(tagEntity);
                            await dbContext.SaveChangesAsync();
                        }

                        // Create the relationship
                        catEntity.CatTags.Add(new CatTag
                        {
                            TagId = tagEntity.Id
                        });
                    }
                }

                // Add the cat to the database
                dbContext.Cats.Add(catEntity);
                await dbContext.SaveChangesAsync();

                _logger.LogInformation($"Added cat {cat.Id} to the database");
            }

            _logger.LogInformation("Cat fetching job completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during cat fetching job");
            throw;
        }
    }
}