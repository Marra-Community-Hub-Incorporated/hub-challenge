using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Data;
using MiniHub.Api.Entities;

namespace MiniHub.Api.Functions;

public record RegisterRequest(string Name, string Email);

public class RegistrationsFunctions
{
    private readonly AppDbContext _db;
    public RegistrationsFunctions(AppDbContext db) => _db = db;

    [Function("Register")]
    public async Task<IActionResult> Register(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "workshops/{id:guid}/register")] HttpRequest req,
        Guid id)
    {
        var body = await req.ReadFromJsonAsync<RegisterRequest>();
        if (body is null || string.IsNullOrWhiteSpace(body.Name) || string.IsNullOrWhiteSpace(body.Email))
            return new BadRequestObjectResult(new { error = "Name and email are required." });

        var workshop = await _db.Workshops.SingleOrDefaultAsync(w => w.Id == id);
        if (workshop is null) return new NotFoundResult();

        var email = body.Email.Trim().ToLowerInvariant();
        var already = await _db.Registrations.AnyAsync(r => r.WorkshopId == id && r.Email == email);
        if (already) return new ConflictObjectResult(new { error = "That email is already registered." });

        var reg = new Registration
        {
            Id = Guid.NewGuid(),
            WorkshopId = id,
            Name = body.Name.Trim(),
            Email = email,
            CreatedAtUtc = DateTime.UtcNow,
        };
        _db.Registrations.Add(reg);
        await _db.SaveChangesAsync();
        return new CreatedResult($"/api/workshops/{id}", new { id = reg.Id });
    }
}
