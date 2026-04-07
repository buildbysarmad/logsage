using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Services;
using LogSage.Api.Services.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;

namespace LogSage.Api.Tests;

public class BillingServiceTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task BillingService_UpdatesUserPlan_WhenWebhookReceived()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            Plan = "free"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var paymentProvider = new Mock<IPaymentProvider>();
        paymentProvider.Setup(p => p.ProviderName).Returns("TestProvider");
        paymentProvider.Setup(p => p.HandleWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandleWebhookResult(
                Success: true,
                UserId: userId.ToString(),
                Plan: "pro",
                ExternalSubscriptionId: "sub_123",
                ExternalCustomerId: "cust_123",
                PriceId: "price_pro",
                Status: "active",
                CurrentPeriodStart: null,
                CurrentPeriodEnd: null,
                CanceledAt: null
            ));

        var logger = new Mock<ILogger<BillingService>>();
        var service = new BillingService(paymentProvider.Object, db, logger.Object);

        // Act
        await service.HandleWebhookAsync("raw-body", "signature");

        // Assert
        var updatedUser = await db.Users.FindAsync(userId);
        Assert.NotNull(updatedUser);
        Assert.Equal("pro", updatedUser.Plan);
        Assert.Equal("cust_123", updatedUser.PaymentCustomerId);
        Assert.Equal("TestProvider", updatedUser.PaymentProvider);
    }

    [Fact]
    public async Task BillingService_IgnoresWebhook_WhenSignatureInvalid()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            Plan = "free"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var paymentProvider = new Mock<IPaymentProvider>();
        paymentProvider.Setup(p => p.ProviderName).Returns("TestProvider");
        paymentProvider.Setup(p => p.HandleWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandleWebhookResult(
                Success: false,
                UserId: null,
                Plan: null,
                ExternalSubscriptionId: null,
                ExternalCustomerId: null,
                PriceId: null,
                Status: null,
                CurrentPeriodStart: null,
                CurrentPeriodEnd: null,
                CanceledAt: null
            ));

        var logger = new Mock<ILogger<BillingService>>();
        var service = new BillingService(paymentProvider.Object, db, logger.Object);

        // Act
        await service.HandleWebhookAsync("raw-body", "invalid-signature");

        // Assert
        var unchangedUser = await db.Users.FindAsync(userId);
        Assert.NotNull(unchangedUser);
        Assert.Equal("free", unchangedUser.Plan); // Plan should remain unchanged
        Assert.Null(unchangedUser.PaymentCustomerId);
    }

    [Fact]
    public async Task BillingService_LogsWarning_WhenUserNotFound()
    {
        // Arrange
        await using var db = CreateInMemoryContext();

        var paymentProvider = new Mock<IPaymentProvider>();
        paymentProvider.Setup(p => p.ProviderName).Returns("TestProvider");
        paymentProvider.Setup(p => p.HandleWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandleWebhookResult(
                Success: true,
                UserId: Guid.NewGuid().ToString(),
                Plan: "pro",
                ExternalSubscriptionId: null,
                ExternalCustomerId: null,
                PriceId: null,
                Status: null,
                CurrentPeriodStart: null,
                CurrentPeriodEnd: null,
                CanceledAt: null
            ));

        var logger = new Mock<ILogger<BillingService>>();
        var service = new BillingService(paymentProvider.Object, db, logger.Object);

        // Act
        await service.HandleWebhookAsync("raw-body", "signature");

        // Assert - Verify warning was logged (no exception thrown)
        logger.Verify(
            l => l.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("not found")),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }

    [Fact]
    public async Task BillingService_CreatesSubscription_WhenWebhookHasSubscriptionData()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "test@example.com",
            PasswordHash = "hash",
            Plan = "free"
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var paymentProvider = new Mock<IPaymentProvider>();
        paymentProvider.Setup(p => p.ProviderName).Returns("Paddle");
        paymentProvider.Setup(p => p.HandleWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new HandleWebhookResult(
                Success: true,
                UserId: userId.ToString(),
                Plan: "pro",
                ExternalSubscriptionId: "sub_456",
                ExternalCustomerId: "cust_456",
                PriceId: "price_pro",
                Status: "active",
                CurrentPeriodStart: DateTime.UtcNow,
                CurrentPeriodEnd: DateTime.UtcNow.AddMonths(1),
                CanceledAt: null
            ));

        var logger = new Mock<ILogger<BillingService>>();
        var service = new BillingService(paymentProvider.Object, db, logger.Object);

        // Act
        await service.HandleWebhookAsync("raw-body", "signature");

        // Assert
        var subscription = await db.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId);
        Assert.NotNull(subscription);
        Assert.Equal("sub_456", subscription.ExternalSubscriptionId);
        Assert.Equal("Paddle", subscription.Provider);
        Assert.Equal("pro", subscription.Plan);
        Assert.Equal("active", subscription.Status);
    }
}
