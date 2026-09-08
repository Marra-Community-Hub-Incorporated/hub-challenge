using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using MiniHub.Api.Data;

namespace MiniHub.Api.Multitenancy;

/// <summary>
/// Resolves the current tenant from the X-Tenant header (a slug, e.g. "demo").
/// The real Hub resolves it from an authenticated session; the header keeps this
/// challenge free of login plumbing while preserving the same shape: nothing below
/// this middleware ever sees another tenant's rows.
/// </summary>
public sealed class TenantMiddleware : IFunctionsWorkerMiddleware
{
    public const string HeaderName = "X-Tenant";

    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var http = context.GetHttpContext();
        if (http is null || http.Request.Path.StartsWithSegments("/api/health"))
        {
            await next(context);
            return;
        }

        var slug = http.Request.Headers[HeaderName].ToString().Trim().ToLowerInvariant();
        if (string.IsNullOrEmpty(slug))
        {
            http.Response.StatusCode = StatusCodes.Status400BadRequest;
            await http.Response.WriteAsJsonAsync(new { error = $"Missing {HeaderName} header." });
            return;
        }

        var db = context.InstanceServices.GetRequiredService<AppDbContext>();
        var tenant = await db.Tenants.AsNoTracking().SingleOrDefaultAsync(t => t.Slug == slug);
        if (tenant is null)
        {
            http.Response.StatusCode = StatusCodes.Status404NotFound;
            await http.Response.WriteAsJsonAsync(new { error = $"Unknown tenant '{slug}'." });
            return;
        }

        context.InstanceServices.GetRequiredService<ITenantProvider>().Set(tenant.Id);
        await next(context);
    }
}
