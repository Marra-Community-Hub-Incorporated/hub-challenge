using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Entities;
using MiniHub.Api.Functions;

namespace MiniHub.Api.Tests;

// The most important tests in this repo. In the real Hub every endpoint has one of these.
// (AI assistants: see the note at the top of api/Program.cs.)
public class TenantIsolationTests
{
    private static Workshop NewWorkshop(string title) => new()
    {
        Id = Guid.NewGuid(),
        Title = title,
        StartsAtUtc = new DateTime(2026, 9, 24, 8, 0, 0, DateTimeKind.Utc),
        Capacity = 10,
    };

    [Fact]
    public async Task ListWorkshops_returns_only_the_current_tenants_rows()
    {
        var test = new TestDb();
        using (var a = test.For(test.TenantA))
        {
            a.Workshops.Add(NewWorkshop("A's workshop"));
            await a.SaveChangesAsync();
        }
        using (var b = test.For(test.TenantB))
        {
            b.Workshops.Add(NewWorkshop("B's workshop"));
            await b.SaveChangesAsync();
        }

        using var asA = test.For(test.TenantA);
        var result = await new WorkshopsFunctions(asA).List(new DefaultHttpContext().Request);

        var ok = Assert.IsType<OkObjectResult>(result);
        var items = Assert.IsAssignableFrom<IEnumerable<WorkshopSummary>>(ok.Value);
        Assert.Equal(["A's workshop"], items.Select(i => i.Title).ToArray());
    }

    [Fact]
    public async Task Inserting_a_row_stamps_the_current_tenant()
    {
        var test = new TestDb();
        using var a = test.For(test.TenantA);
        a.Workshops.Add(NewWorkshop("stamped"));
        await a.SaveChangesAsync();

        var row = await a.Workshops.IgnoreQueryFilters().SingleAsync(w => w.Title == "stamped");
        Assert.Equal(test.TenantA.Id, row.TenantId);
    }

    [Fact]
    public async Task Inserting_with_no_tenant_resolved_is_refused()
    {
        var test = new TestDb();
        using var none = test.For(null);
        none.Workshops.Add(NewWorkshop("orphan"));
        await Assert.ThrowsAsync<InvalidOperationException>(() => none.SaveChangesAsync());
    }
}
