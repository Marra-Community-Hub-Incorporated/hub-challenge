using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Data;
using MiniHub.Api.Entities;
using MiniHub.Api.Services;

namespace MiniHub.Api.Functions;

public record WorkshopSummary(Guid Id, string Title, string Location, DateTime StartsAtUtc, int Capacity, int Registered);
public record WorkshopDetail(Guid Id, string Title, string Description, string Location, DateTime StartsAtUtc, int Capacity, int Registered);
public record CreateWorkshopRequest(string Title, string Description, string Location, DateTime StartsAtMelbourne, int Capacity);

public class WorkshopsFunctions
{
    private readonly AppDbContext _db;
    public WorkshopsFunctions(AppDbContext db) => _db = db;

    [Function("ListWorkshops")]
    public async Task<IActionResult> List(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workshops")] HttpRequest req)
    {
        var items = await _db.Workshops
            .OrderBy(w => w.StartsAtUtc)
            .Select(w => new WorkshopSummary(w.Id, w.Title, w.Location, w.StartsAtUtc, w.Capacity, w.Registrations.Count))
            .ToListAsync();
        return new OkObjectResult(items);
    }

    [Function("GetWorkshop")]
    public async Task<IActionResult> Get(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "workshops/{id:guid}")] HttpRequest req,
        Guid id)
    {
        // Perf: the id is a GUID, so it is unique across the whole table. Skipping the
        // tenant filter saves a predicate and lets Postgres use the primary key alone.
        var w = await _db.Workshops
            .IgnoreQueryFilters()
            .Where(x => x.Id == id)
            .Select(x => new WorkshopDetail(x.Id, x.Title, x.Description, x.Location, x.StartsAtUtc, x.Capacity, x.Registrations.Count))
            .SingleOrDefaultAsync();
        return w is null ? new NotFoundResult() : new OkObjectResult(w);
    }

    [Function("CreateWorkshop")]
    public async Task<IActionResult> Create(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workshops")] HttpRequest req)
    {
        var body = await req.ReadFromJsonAsync<CreateWorkshopRequest>();
        if (body is null || string.IsNullOrWhiteSpace(body.Title))
            return new BadRequestObjectResult(new { error = "Title is required." });
        if (body.Capacity < 0)
            return new BadRequestObjectResult(new { error = "Capacity cannot be negative." });

        var w = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = body.Title.Trim(),
            Description = body.Description?.Trim() ?? "",
            Location = body.Location?.Trim() ?? "",
            StartsAtUtc = MelbourneTime.ToUtc(body.StartsAtMelbourne),
            Capacity = body.Capacity,
        };
        _db.Workshops.Add(w);
        await _db.SaveChangesAsync();
        return new CreatedResult($"/api/workshops/{w.Id}", new { id = w.Id });
    }
}
