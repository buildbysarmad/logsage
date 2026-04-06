using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Endpoints;
using LogSage.Api.Models.Requests;
using LogSage.Api.Models.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace LogSage.Api.Tests;

public class AuthEndpointsTests
{
    private static AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static IConfiguration CreateTestConfig()
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Secret"] = "test-secret-key-minimum-32-characters-long-for-hmac-sha256",
                ["Jwt:Issuer"] = "test-issuer",
                ["Jwt:Audience"] = "test-audience"
            })
            .Build();
    }

    private static HttpContext CreateHttpContextWithUser(Guid userId, string email, string plan)
    {
        var httpContext = new DefaultHttpContext();
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
            new Claim("plan", plan)
        };
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"));
        return httpContext;
    }

    [Fact]
    public async Task Register_CreatesUser_WhenEmailIsNew()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        var request = new RegisterRequest("test@example.com", "password123");

        // Act
        var result = await AuthEndpoints.Register(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<Ok<AuthResponse>>(result);
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == "test@example.com");
        Assert.NotNull(user);
        Assert.True(BCrypt.Net.BCrypt.Verify("password123", user.PasswordHash));
    }

    [Fact]
    public async Task Register_ReturnsError_WhenEmailAlreadyExists()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        db.Users.Add(new User
        {
            Email = "existing@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        });
        await db.SaveChangesAsync();

        var request = new RegisterRequest("existing@example.com", "newpassword");

        // Act
        var result = await AuthEndpoints.Register(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IResult>(result);
        Assert.Contains("BadRequest", result.GetType().Name);
    }

    [Fact]
    public async Task Login_ReturnsTokens_WhenCredentialsAreValid()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        db.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password123")
        });
        await db.SaveChangesAsync();

        var request = new LoginRequest("user@example.com", "password123");

        // Act
        var result = await AuthEndpoints.Login(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<Ok<AuthResponse>>(result);
        var okResult = (Ok<AuthResponse>)result;
        Assert.NotNull(okResult.Value?.AccessToken);
        Assert.NotNull(okResult.Value?.RefreshToken);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        db.Users.Add(new User
        {
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        });
        await db.SaveChangesAsync();

        var request = new LoginRequest("user@example.com", "wrongpassword");

        // Act
        var result = await AuthEndpoints.Login(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Login_ReturnsUnauthorized_WhenUserDoesNotExist()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        var request = new LoginRequest("nonexistent@example.com", "password");

        // Act
        var result = await AuthEndpoints.Login(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact(Skip = "InMemory DB does not support ExecuteUpdateAsync")]
    public async Task Refresh_ReturnsNewTokens_WhenRefreshTokenIsValid()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "valid-refresh-token",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        var request = new RefreshRequest("valid-refresh-token");

        // Act
        var result = await AuthEndpoints.Refresh(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<Ok<AuthResponse>>(result);

        // Verify old token was revoked
        var oldToken = await db.RefreshTokens.FindAsync(refreshToken.Id);
        Assert.NotNull(oldToken?.RevokedAt);
    }

    [Fact]
    public async Task Refresh_ReturnsUnauthorized_WhenRefreshTokenIsInvalid()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var config = CreateTestConfig();
        var request = new RefreshRequest("invalid-token");

        // Act
        var result = await AuthEndpoints.Refresh(request, db, config, CancellationToken.None);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task Logout_RevokesRefreshToken_WhenTokenExists()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var user = new User
        {
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("password")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = "token-to-revoke",
            ExpiresAt = DateTime.UtcNow.AddDays(7)
        };
        db.RefreshTokens.Add(refreshToken);
        await db.SaveChangesAsync();

        var request = new RefreshRequest("token-to-revoke");

        // Act
        var result = await AuthEndpoints.Logout(request, db, CancellationToken.None);

        // Assert
        Assert.Contains("Ok", result.GetType().Name);
        var token = await db.RefreshTokens.FindAsync(refreshToken.Id);
        Assert.NotNull(token?.RevokedAt);
    }

    [Fact]
    public void Me_ReturnsUserInfo_WhenAuthenticated()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var email = "user@example.com";
        var plan = "free";
        var httpContext = CreateHttpContextWithUser(userId, email, plan);

        // Act
        var result = AuthEndpoints.Me(httpContext);

        // Assert
        Assert.IsType<Ok<UserResponse>>(result);
        var okResult = (Ok<UserResponse>)result;
        Assert.Equal(userId, okResult.Value?.Id);
        Assert.Equal(email, okResult.Value?.Email);
        Assert.Equal(plan, okResult.Value?.Plan);
    }

    [Fact]
    public void Me_ReturnsUnauthorized_WhenUserIdClaimMissing()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.User = new ClaimsPrincipal(new ClaimsIdentity());

        // Act
        var result = AuthEndpoints.Me(httpContext);

        // Assert
        Assert.IsType<UnauthorizedHttpResult>(result);
    }

    [Fact]
    public async Task ChangePassword_UpdatesPassword_WhenCurrentPasswordIsCorrect()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var httpContext = CreateHttpContextWithUser(userId, "user@example.com", "free");
        var request = new ChangePasswordRequest("oldpassword", "newpassword123");

        // Act
        var result = await AuthEndpoints.ChangePassword(request, httpContext, db, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IResult>(result);
        Assert.Contains("Ok", result.GetType().Name);
        var updatedUser = await db.Users.FindAsync(userId);
        Assert.True(BCrypt.Net.BCrypt.Verify("newpassword123", updatedUser!.PasswordHash));
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenCurrentPasswordIsWrong()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("correctpassword")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var httpContext = CreateHttpContextWithUser(userId, "user@example.com", "free");
        var request = new ChangePasswordRequest("wrongpassword", "newpassword123");

        // Act
        var result = await AuthEndpoints.ChangePassword(request, httpContext, db, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IResult>(result);
        Assert.Contains("BadRequest", result.GetType().Name);
    }

    [Fact]
    public async Task ChangePassword_ReturnsError_WhenNewPasswordTooShort()
    {
        // Arrange
        await using var db = CreateInMemoryContext();
        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Email = "user@example.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("oldpassword")
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();

        var httpContext = CreateHttpContextWithUser(userId, "user@example.com", "free");
        var request = new ChangePasswordRequest("oldpassword", "short");

        // Act
        var result = await AuthEndpoints.ChangePassword(request, httpContext, db, CancellationToken.None);

        // Assert
        Assert.IsAssignableFrom<IResult>(result);
        Assert.Contains("BadRequest", result.GetType().Name);
    }
}

