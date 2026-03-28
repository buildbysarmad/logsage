using LogSage.Api.Services;

namespace LogSage.Api.Endpoints;

public static class BillingEndpoints
{
    public static void MapBillingEndpoints(this WebApplication app)
    {
        app.MapPost("/api/billing/checkout", CreateCheckout).WithTags("Billing").WithSummary("Create Paddle checkout session").RequireAuthorization();
        app.MapPost("/api/billing/webhook", HandleWebhook).WithTags("Billing").WithSummary("Paddle webhook events handler");
        app.MapGet("/api/billing/portal", GetPortal).WithTags("Billing").WithSummary("Get Paddle billing portal URL").RequireAuthorization();
    }

    private static async Task<IResult> CreateCheckout(
        CheckoutRequest req, StripeService stripe,
        HttpContext ctx, IConfiguration config)
    {
        var url = await stripe.CreateCheckoutSessionAsync(
            ctx.User.FindFirst("sub")?.Value!,
            req.PriceId,
            config["App:BaseUrl"] + "/billing/success",
            config["App:BaseUrl"] + "/billing/cancel");
        return Results.Ok(new { checkoutUrl = url });
    }

    private static async Task<IResult> HandleWebhook(
        HttpRequest request, StripeService stripe)
    {
        using (var reader = new StreamReader(request.Body, leaveOpen: true))
        {
            var json = await reader.ReadToEndAsync();
            await stripe.HandleWebhookAsync(json, request.Headers["Stripe-Signature"].ToString());
        }
        return Results.Ok();
    }

    private static IResult GetPortal() =>
        Results.Ok(new { message = "Billing portal coming soon" });
}

public record CheckoutRequest(string PriceId);
