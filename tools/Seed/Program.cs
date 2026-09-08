using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Data;
using MiniHub.Api.Entities;
using MiniHub.Api.Multitenancy;
using MiniHub.Api.Services;

// Seeds two organisations into the local Docker Postgres and prints your challenge token.
// Local-dev tool only; never deployed. Run with: npm run db:seed
//
// Set CHALLENGE_ID to the id from your invitation email, e.g.
//   CHALLENGE_ID=abc123 npm run db:seed          (macOS / Linux)
//   $env:CHALLENGE_ID="abc123"; npm run db:seed  (PowerShell)

var connectionString = ConnectionStrings.Resolve(Environment.GetEnvironmentVariable("ConnectionStrings__Postgres"));
var options = new DbContextOptionsBuilder<AppDbContext>().UseNpgsql(connectionString).Options;

Tenant demo, other;
using (var db = new AppDbContext(options, new TenantProvider()))
{
    await db.Database.MigrateAsync();
    demo = await Upsert(db, "demo", "Demo Community Hub");
    other = await Upsert(db, "other-org", "Some Other Organisation");
}

await SeedTenant(demo, [
    // Times below are Melbourne wall-clock, as a coordinator would type them in.
    ("Intro to Volunteering", "What volunteering with us looks like, week to week.", "Carnegie Library, Level 2", new DateTime(2026, 9, 24, 18, 0, 0), 12, 4),
    ("Food Pantry Shift Training", "Handling, storage, and the sign-in sheet.", "Caulfield South Community House", new DateTime(2026, 10, 3, 10, 30, 0), 8, 8),
    ("First Aid Refresher", "CPR and the basics. Certificate issued.", "Glen Eira Town Hall", new DateTime(2026, 10, 17, 9, 0, 0), 20, 2),
    ("Committee Q&A", "Ask the committee anything.", "Online", new DateTime(2026, 11, 5, 19, 0, 0), 0, 0),
]);

await SeedTenant(other, [
    ("Other Org's Private Workshop", "You should never see this from the demo tenant.", "Somewhere else", new DateTime(2026, 9, 30, 12, 0, 0), 5, 1),
]);

var challengeId = Environment.GetEnvironmentVariable("CHALLENGE_ID")?.Trim();
using (var db = new AppDbContext(options, new TenantProvider()))
{
    var run = new ChallengeRun
    {
        Id = Guid.NewGuid(),
        ChallengeId = string.IsNullOrEmpty(challengeId) ? "(none)" : challengeId,
        CreatedAtUtc = DateTime.UtcNow,
    };
    db.ChallengeRuns.Add(run);
    await db.SaveChangesAsync();

    Console.WriteLine();
    Console.WriteLine("Seeded: demo (4 workshops) and other-org (1 workshop).");
    Console.WriteLine("Frontend: pick the tenant in the top-right. Login is not needed.");
    Console.WriteLine();
    if (string.IsNullOrEmpty(challengeId))
    {
        Console.WriteLine("No CHALLENGE_ID set, so no token was issued. Set it and run the seed again");
        Console.WriteLine("before you submit (the seed is idempotent; run it as often as you like).");
    }
    else
    {
        Console.WriteLine($"Challenge token: {Token(run)}");
        Console.WriteLine("Paste that line into your pull request description.");
    }
    Console.WriteLine();
}

static string Token(ChallengeRun run)
{
    var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{run.ChallengeId}:{run.Id:N}"));
    var hex = Convert.ToHexString(hash)[..12];
    return $"{hex[..4]}-{hex[4..8]}-{hex[8..]}";
}

static async Task<Tenant> Upsert(AppDbContext db, string slug, string name)
{
    var t = await db.Tenants.SingleOrDefaultAsync(x => x.Slug == slug);
    if (t is null)
    {
        t = new Tenant { Id = Guid.NewGuid(), Slug = slug, Name = name };
        db.Tenants.Add(t);
        await db.SaveChangesAsync();
    }
    return t;
}

async Task SeedTenant(Tenant tenant, (string Title, string Description, string Location, DateTime StartsMelbourne, int Capacity, int Registrations)[] rows)
{
    var provider = new TenantProvider();
    provider.Set(tenant.Id);
    using var db = new AppDbContext(options, provider);

    // Idempotent: wipe this tenant's rows (filtered by the tenant query filter) and re-insert.
    db.Registrations.RemoveRange(await db.Registrations.ToListAsync());
    db.Workshops.RemoveRange(await db.Workshops.ToListAsync());
    await db.SaveChangesAsync();

    foreach (var r in rows)
    {
        var w = new Workshop
        {
            Id = Guid.NewGuid(),
            Title = r.Title,
            Description = r.Description,
            Location = r.Location,
            StartsAtUtc = MelbourneTime.ToUtc(r.StartsMelbourne),
            Capacity = r.Capacity,
        };
        for (var i = 1; i <= r.Registrations; i++)
        {
            w.Registrations.Add(new Registration
            {
                Id = Guid.NewGuid(),
                Name = $"Volunteer {i}",
                Email = $"volunteer{i}@example.org",
                CreatedAtUtc = DateTime.UtcNow.AddDays(-i),
            });
        }
        db.Workshops.Add(w);
    }
    await db.SaveChangesAsync();
    Console.WriteLine($"[{tenant.Slug}] {rows.Length} workshop(s)");
}
