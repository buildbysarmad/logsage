namespace LogSage.Api.Services.Payments;

public interface IPaymentProvider
{
    string ProviderName { get; }
    Task<CreateCheckoutResult> CreateCheckoutAsync(CreateCheckoutRequest request, CancellationToken ct = default);
    Task<HandleWebhookResult> HandleWebhookAsync(string rawBody, string signature, CancellationToken ct = default);
    Task<string> GetPortalUrlAsync(string customerId, CancellationToken ct = default);
}

public record CreateCheckoutRequest(
    string UserId,
    string PriceId,
    string SuccessUrl,
    string CancelUrl);

public record CreateCheckoutResult(
    string CheckoutUrl);

public record HandleWebhookResult(
    bool Success,
    string? UserId,
    string? Plan,
    string? ExternalSubscriptionId,
    string? ExternalCustomerId,
    string? PriceId,
    string? Status,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    DateTime? CanceledAt);
