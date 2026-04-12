using System.Security.Cryptography;
using System.Text;

namespace LogSage.Api.Infrastructure;

public class AdminKeyFilter(IConfiguration config) : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var adminKey = config["AdminApiKey"];

        // In development, allow empty admin key
        if (string.IsNullOrWhiteSpace(adminKey) && !context.HttpContext.RequestServices
            .GetRequiredService<IHostEnvironment>().IsDevelopment())
        {
            return Results.Problem("Admin key not configured", statusCode: 500);
        }

        // Get header value
        if (!httpContext.Request.Headers.TryGetValue("X-Admin-Key", out var providedKey) ||
            string.IsNullOrWhiteSpace(providedKey))
        {
            return Results.Unauthorized();
        }

        // Timing-safe comparison
        if (!string.IsNullOrWhiteSpace(adminKey) && !TimingSafeEquals(adminKey, providedKey!))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }

    private static bool TimingSafeEquals(string expected, string provided)
    {
        var expectedBytes = Encoding.UTF8.GetBytes(expected);
        var providedBytes = Encoding.UTF8.GetBytes(provided);

        if (expectedBytes.Length != providedBytes.Length)
            return false;

        return CryptographicOperations.FixedTimeEquals(expectedBytes, providedBytes);
    }
}
