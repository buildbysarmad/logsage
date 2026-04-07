using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Services.Payments;
using Microsoft.EntityFrameworkCore;

namespace LogSage.Api.Services;

public class BillingService(
    IPaymentProvider paymentProvider,
    AppDbContext db,
    ILogger<BillingService> logger)
{
    public async Task<string> CreateCheckoutAsync(
        string userId, string priceId,
        string successUrl, string cancelUrl,
        CancellationToken ct = default)
    {
        var request = new CreateCheckoutRequest(userId, priceId, successUrl, cancelUrl);
        var result = await paymentProvider.CreateCheckoutAsync(request, ct);
        return result.CheckoutUrl;
    }

    public async Task HandleWebhookAsync(string rawBody, string signature, CancellationToken ct = default)
    {
        var result = await paymentProvider.HandleWebhookAsync(rawBody, signature, ct);

        if (!result.Success || result.UserId == null)
        {
            logger.LogWarning("Webhook processing failed or missing user ID");
            return;
        }

        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Id.ToString() == result.UserId, ct);

        if (user == null)
        {
            logger.LogWarning("User {UserId} not found for webhook event", result.UserId);
            return;
        }

        // Update user plan and payment info
        user.Plan = result.Plan ?? "free";
        user.PaymentCustomerId = result.ExternalCustomerId;
        user.PaymentProvider = paymentProvider.ProviderName;
        user.UpdatedAt = DateTime.UtcNow;

        // Upsert subscription record
        if (result.ExternalSubscriptionId != null)
        {
            var subscription = await db.Subscriptions.FirstOrDefaultAsync(
                s => s.ExternalSubscriptionId == result.ExternalSubscriptionId, ct);

            if (subscription == null)
            {
                subscription = new Subscription
                {
                    UserId = user.Id,
                    Provider = paymentProvider.ProviderName,
                    ExternalSubscriptionId = result.ExternalSubscriptionId,
                    ExternalCustomerId = result.ExternalCustomerId ?? string.Empty,
                    PriceId = result.PriceId ?? string.Empty,
                    Plan = result.Plan ?? "free",
                    Status = result.Status ?? "active",
                    CurrentPeriodStart = result.CurrentPeriodStart,
                    CurrentPeriodEnd = result.CurrentPeriodEnd,
                    CanceledAt = result.CanceledAt,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
                db.Subscriptions.Add(subscription);
            }
            else
            {
                subscription.Status = result.Status ?? subscription.Status;
                subscription.Plan = result.Plan ?? subscription.Plan;
                subscription.CurrentPeriodStart = result.CurrentPeriodStart ?? subscription.CurrentPeriodStart;
                subscription.CurrentPeriodEnd = result.CurrentPeriodEnd ?? subscription.CurrentPeriodEnd;
                subscription.CanceledAt = result.CanceledAt ?? subscription.CanceledAt;
                subscription.UpdatedAt = DateTime.UtcNow;
            }
        }

        await db.SaveChangesAsync(ct);
        logger.LogInformation("Updated user {UserId} to plan {Plan}", result.UserId, result.Plan);
    }

    public async Task<string> GetPortalUrlAsync(string userId, CancellationToken ct = default)
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Id.ToString() == userId, ct);

        if (user?.PaymentCustomerId == null)
        {
            throw new InvalidOperationException("User has no payment customer ID");
        }

        return await paymentProvider.GetPortalUrlAsync(user.PaymentCustomerId, ct);
    }
}
