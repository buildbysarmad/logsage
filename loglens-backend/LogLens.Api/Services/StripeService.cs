using Stripe;
using Stripe.Checkout;
using LogLens.Api.Data;
using Microsoft.EntityFrameworkCore;

namespace LogLens.Api.Services;

public class StripeService(IConfiguration config, AppDbContext db)
{
    public async Task<string> CreateCheckoutSessionAsync(
        string userId, string priceId,
        string successUrl, string cancelUrl)
    {
        StripeConfiguration.ApiKey = config["Stripe:SecretKey"];
        var options = new SessionCreateOptions
        {
            Mode = "subscription",
            LineItems = [new SessionLineItemOptions { Price = priceId, Quantity = 1 }],
            SuccessUrl = successUrl + "?session_id={CHECKOUT_SESSION_ID}",
            CancelUrl = cancelUrl,
            Metadata = new Dictionary<string, string> { ["userId"] = userId }
        };
        var session = await new Stripe.Checkout.SessionService().CreateAsync(options);
        return session.Url;
    }

    public async Task HandleWebhookAsync(string json, string signature)
    {
        var stripeEvent = EventUtility.ConstructEvent(
            json, signature, config["Stripe:WebhookSecret"]!);

        if (stripeEvent.Data.Object is Subscription sub)
        {
            var userId = sub.Metadata.GetValueOrDefault("userId");
            if (userId == null) return;
            var plan = stripeEvent.Type == "customer.subscription.deleted" ? "free" : "pro";
            var user = await db.Users.FirstOrDefaultAsync(u => u.Id.ToString() == userId);
            if (user != null)
            {
                user.Plan = plan;
                user.UpdatedAt = DateTime.UtcNow;
                await db.SaveChangesAsync();
            }
        }
    }
}
