using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace LogSage.Api.Services.Payments;

/// <summary>
/// Paddle payment provider implementation.
/// Configuration keys required:
/// - Paddle:ApiKey → Get from: vendors.paddle.com → Developer Tools → Authentication
/// - Paddle:WebhookSecret → Get from: vendors.paddle.com → Developer Tools → Notifications → endpoint secret
/// - Paddle:ProPriceId → Get from: vendors.paddle.com → Catalog → Prices → Pro plan Price ID
/// - Paddle:TeamPriceId → Get from: vendors.paddle.com → Catalog → Prices → Team plan Price ID
/// - Paddle:VendorId → Get from: vendors.paddle.com → Settings → Vendor ID
/// </summary>
public class PaddlePaymentProvider(
    IConfiguration configuration,
    IHttpClientFactory httpClientFactory,
    ILogger<PaddlePaymentProvider> logger) : IPaymentProvider
{
    public string ProviderName => "paddle";

    public async Task<CreateCheckoutResult> CreateCheckoutAsync(
        CreateCheckoutRequest request, CancellationToken ct = default)
    {
        var httpClient = httpClientFactory.CreateClient();
        var apiKey = configuration["Paddle:ApiKey"];

        var payload = new
        {
            items = new[]
            {
                new { price_id = request.PriceId, quantity = 1 }
            },
            custom_data = new { user_id = request.UserId },
            success_url = request.SuccessUrl,
            cancel_url = request.CancelUrl
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "https://api.paddle.com/transactions")
        {
            Headers = { { "Authorization", $"Bearer {apiKey}" } },
            Content = JsonContent.Create(payload)
        };

        var response = await httpClient.SendAsync(httpRequest, ct);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PaddleCheckoutResponse>(ct);
        logger.LogInformation("Paddle checkout created for user {UserId}", request.UserId);

        return new CreateCheckoutResult(result?.Data?.Url ?? throw new InvalidOperationException("No checkout URL returned from Paddle"));
    }

    public async Task<HandleWebhookResult> HandleWebhookAsync(
        string rawBody, string signature, CancellationToken ct = default)
    {
        var secret = configuration["Paddle:WebhookSecret"];
        if (!VerifySignature(rawBody, signature, secret!))
        {
            logger.LogWarning("Invalid Paddle webhook signature");
            return new HandleWebhookResult(false, null, null, null, null, null, null, null, null, null);
        }

        var webhookEvent = JsonSerializer.Deserialize<PaddleWebhookEvent>(rawBody);
        if (webhookEvent?.EventType == null)
        {
            return new HandleWebhookResult(false, null, null, null, null, null, null, null, null, null);
        }

        logger.LogInformation("Received Paddle webhook: {EventType}", webhookEvent.EventType);

        var userId = webhookEvent.Data?.CustomData?.GetProperty("user_id").GetString();
        var priceId = webhookEvent.Data?.Items?[0].Price?.Id;
        var plan = ResolvePlanFromPriceId(priceId);
        var status = MapPaddleStatus(webhookEvent.EventType, webhookEvent.Data?.Status);
        var subscriptionId = webhookEvent.Data?.SubscriptionId;
        var customerId = webhookEvent.Data?.CustomerId;
        var currentPeriodStart = webhookEvent.Data?.CurrentBillingPeriod?.StartsAt;
        var currentPeriodEnd = webhookEvent.Data?.CurrentBillingPeriod?.EndsAt;
        var canceledAt = webhookEvent.Data?.CanceledAt;

        return new HandleWebhookResult(
            true, userId, plan, subscriptionId, customerId, priceId,
            status, currentPeriodStart, currentPeriodEnd, canceledAt);
    }

    public Task<string> GetPortalUrlAsync(string customerId, CancellationToken ct = default)
    {
        // Paddle customer portal URL - customers can manage subscriptions here
        return Task.FromResult("https://customer.paddle.com/subscriptions");
    }

    private bool VerifySignature(string rawBody, string signature, string secret)
    {
        // Paddle sends signature in format: ts=timestamp;h1=hash
        var parts = signature.Split(';');
        var timestamp = parts.FirstOrDefault(p => p.StartsWith("ts="))?.Substring(3);
        var hash = parts.FirstOrDefault(p => p.StartsWith("h1="))?.Substring(3);

        if (string.IsNullOrEmpty(timestamp) || string.IsNullOrEmpty(hash))
            return false;

        var payload = $"{timestamp}:{rawBody}";
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var computedHash = Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(payload))).ToLower();

        return computedHash == hash;
    }

    private string ResolvePlanFromPriceId(string? priceId)
    {
        if (priceId == configuration["Paddle:ProPriceId"]) return "pro";
        if (priceId == configuration["Paddle:TeamPriceId"]) return "team";
        return "free";
    }

    private static string MapPaddleStatus(string eventType, string? paddleStatus)
    {
        return eventType switch
        {
            "subscription.created" => "active",
            "subscription.activated" => "active",
            "subscription.updated" => paddleStatus ?? "active",
            "subscription.canceled" => "canceled",
            "subscription.paused" => "paused",
            "subscription.past_due" => "past_due",
            _ => "unknown"
        };
    }

    // Paddle API response models
    private record PaddleCheckoutResponse(PaddleCheckoutData? Data);
    private record PaddleCheckoutData(string Url);

    private record PaddleWebhookEvent(
        string? EventType,
        PaddleSubscriptionData? Data);

    private record PaddleSubscriptionData(
        string? SubscriptionId,
        string? CustomerId,
        string? Status,
        PaddleItem[]? Items,
        JsonElement? CustomData,
        PaddleBillingPeriod? CurrentBillingPeriod,
        DateTime? CanceledAt);

    private record PaddleItem(PaddlePrice? Price);
    private record PaddlePrice(string? Id);
    private record PaddleBillingPeriod(DateTime? StartsAt, DateTime? EndsAt);
}
