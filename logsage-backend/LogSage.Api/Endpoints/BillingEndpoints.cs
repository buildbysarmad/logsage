using LogSage.Api.Services;

namespace LogSage.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/billing/checkout", CreateCheckout).WithTags("Billing").WithSummary("Create payment checkout session").RequireAuthorization();
        app.MapPost("/api/billing/webhook", HandleWebhook).WithTags("Billing").WithSummary("Payment webhook events handler");
        app.MapGet("/api/billing/portal", GetPortal).WithTags("Billing").WithSummary("Get billing portal URL").RequireAuthorization();
    }

    private static async Task<IResult> CreateCheckout(
        CheckoutRequest req, BillingService billing,
        HttpContext ctx, IConfiguration config, CancellationToken ct)
    {
        var userId = ctx.User.FindFirst("sub")?.Value!;
        var successUrl = config["App:BaseUrl"] + "/billing/success";
        var cancelUrl = config["App:BaseUrl"] + "/billing/cancel";

        var checkoutUrl = await billing.CreateCheckoutAsync(
            userId, req.PriceId, successUrl, cancelUrl, ct);

        return Results.Ok(new { checkoutUrl });
    }

    private static async Task<IResult> HandleWebhook(
        HttpRequest request, BillingService billing, CancellationToken ct)
    {
        using var reader = new StreamReader(request.Body, leaveOpen: true);
        var rawBody = await reader.ReadToEndAsync(ct);
        var signature = request.Headers["Paddle-Signature"].ToString();

        await billing.HandleWebhookAsync(rawBody, signature, ct);
        return Results.Ok();
    }

    private static async Task<IResult> GetPortal(
        HttpContext ctx, BillingService billing, CancellationToken ct)
    {
        var userId = ctx.User.FindFirst("sub")?.Value!;
        var portalUrl = await billing.GetPortalUrlAsync(userId, ct);
        return Results.Ok(new { portalUrl });
    }
}

public record CheckoutRequest(string PriceId);
