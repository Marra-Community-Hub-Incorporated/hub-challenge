using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHub.Api.Entities;
using MiniHub.Api.Functions;

namespace MiniHub.Api.Tests;

public class RegistrationTests
{
    private static HttpRequest JsonRequest(object body)
    {
        var ctx = new DefaultHttpContext();
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(body));
        ctx.Request.Body = new MemoryStream(bytes);
        ctx.Request.ContentType = "application/json";
        ctx.Request.ContentLength = bytes.Length;
        return ctx.Request;
    }

    [Fact]
    public async Task Register_creates_a_registration_for_the_workshop()
    {
        var test = new TestDb();
        var workshop = new Workshop { Id = Guid.NewGuid(), Title = "w", StartsAtUtc = DateTime.UtcNow, Capacity = 5 };
        using (var a = test.For(test.TenantA))
        {
            a.Workshops.Add(workshop);
            await a.SaveChangesAsync();
        }

        using var asA = test.For(test.TenantA);
        var result = await new RegistrationsFunctions(asA)
            .Register(JsonRequest(new { name = "Sam", email = "Sam@Example.org" }), workshop.Id);

        Assert.IsType<CreatedResult>(result);
        var reg = await asA.Registrations.SingleAsync();
        Assert.Equal("sam@example.org", reg.Email);
        Assert.Equal(test.TenantA.Id, reg.TenantId);
    }

    [Fact]
    public async Task Register_twice_with_the_same_email_is_a_conflict()
    {
        var test = new TestDb();
        var workshop = new Workshop { Id = Guid.NewGuid(), Title = "w", StartsAtUtc = DateTime.UtcNow, Capacity = 5 };
        using (var a = test.For(test.TenantA))
        {
            a.Workshops.Add(workshop);
            await a.SaveChangesAsync();
        }

        using var asA = test.For(test.TenantA);
        var fn = new RegistrationsFunctions(asA);
        await fn.Register(JsonRequest(new { name = "Sam", email = "sam@example.org" }), workshop.Id);
        var second = await fn.Register(JsonRequest(new { name = "Sam", email = "SAM@example.org" }), workshop.Id);

        Assert.IsType<ConflictObjectResult>(second);
    }
}
