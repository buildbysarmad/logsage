using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using LogSage.Api.Data;
using LogSage.Api.Data.Entities;
using LogSage.Api.Models.Requests;
using LogSage.Api.Models.Responses;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace LogSage.Api.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this WebApplication app)
    {
        app.MapPost("/api/auth/register", Register).WithTags("Auth").WithSummary("Register a new account");
        app.MapPost("/api/auth/login", Login).WithTags("Auth").WithSummary("Login and receive JWT tokens");
        app.MapPost("/api/auth/refresh", Refresh).WithTags("Auth").WithSummary("Refresh access token using refresh token");
        app.MapPost("/api/auth/logout", Logout).WithTags("Auth").WithSummary("Revoke refresh token");
        app.MapGet("/api/auth/me", Me).WithTags("Auth").WithSummary("Get current authenticated user").RequireAuthorization();
        app.MapPost("/api/auth/change-password", ChangePassword).WithTags("Auth").WithSummary("Change user password").RequireAuthorization();
    }

    private static async Task<IResult> Register(
        RegisterRequest req, AppDbContext db,
        IConfiguration config, CancellationToken ct)
    {
        if (await db.Users.AnyAsync(u => u.Email == req.Email, ct))
            return Results.BadRequest(new { message = "Email already registered" });

        var user = new User {
            Email = req.Email.ToLowerInvariant(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.Password)
        };
        db.Users.Add(user);
        await db.SaveChangesAsync(ct);
        return Results.Ok(await GenerateTokensAsync(user, db, config, ct));
    }

    private static async Task<IResult> Login(
        LoginRequest req, AppDbContext db,
        IConfiguration config, CancellationToken ct)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == req.Email.ToLowerInvariant(), ct);
        if (user == null || !BCrypt.Net.BCrypt.Verify(req.Password, user.PasswordHash))
            return Results.Unauthorized();
        return Results.Ok(await GenerateTokensAsync(user, db, config, ct));
    }

    private static async Task<IResult> Refresh(
        RefreshRequest req, AppDbContext db,
        IConfiguration config, CancellationToken ct)
    {
        var token = await db.RefreshTokens.Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == req.RefreshToken, ct);
        if (token == null || !token.IsActive) return Results.Unauthorized();

        // Atomic revocation to prevent concurrent use
        var revoked = await db.RefreshTokens
            .Where(r => r.Id == token.Id && r.RevokedAt == null)
            .ExecuteUpdateAsync(r => r.SetProperty(x => x.RevokedAt, DateTime.UtcNow), ct);

        if (revoked == 0)
            return Results.Unauthorized(); // Already revoked by concurrent request

        return Results.Ok(await GenerateTokensAsync(token.User, db, config, ct));
    }

    private static async Task<IResult> Logout(
        RefreshRequest req, AppDbContext db, CancellationToken ct)
    {
        var token = await db.RefreshTokens
            .FirstOrDefaultAsync(r => r.Token == req.RefreshToken, ct);
        if (token != null) { token.RevokedAt = DateTime.UtcNow; await db.SaveChangesAsync(ct); }
        return Results.Ok();
    }

    private static IResult Me(HttpContext ctx)
    {
        var userIdStr = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                     ?? ctx.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                     ?? ctx.User.FindFirst("sub")?.Value;

        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
            return Results.Unauthorized();

        var email = ctx.User.FindFirst(ClaimTypes.Email)?.Value;
        var plan = ctx.User.FindFirst("plan")?.Value;

        if (email == null || plan == null)
            return Results.Unauthorized();

        return Results.Ok(new UserResponse(userId, email, plan));
    }

    private static async Task<IResult> ChangePassword(
        ChangePasswordRequest req, HttpContext ctx,
        AppDbContext db, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.CurrentPassword) || string.IsNullOrWhiteSpace(req.NewPassword))
            return Results.BadRequest(new { error = "Current password and new password are required" });

        if (req.NewPassword.Length < 8)
            return Results.BadRequest(new { error = "New password must be at least 8 characters" });

        var userIdStr = ctx.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
        if (userIdStr == null || !Guid.TryParse(userIdStr, out var userId))
            return Results.Unauthorized();

        var user = await db.Users.FindAsync([userId], ct);
        if (user == null)
            return Results.Unauthorized();

        if (!BCrypt.Net.BCrypt.Verify(req.CurrentPassword, user.PasswordHash))
            return Results.BadRequest(new { error = "Current password is incorrect" });

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(req.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return Results.Ok(new { message = "Password changed successfully" });
    }

    private static async Task<AuthResponse> GenerateTokensAsync(
        User user, AppDbContext db, IConfiguration config, CancellationToken ct)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Secret"]!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var claims = new[] {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim("plan", user.Plan)
        };
        var accessToken = new JwtSecurityTokenHandler().WriteToken(
            new JwtSecurityToken(config["Jwt:Issuer"], config["Jwt:Audience"],
                claims, expires: DateTime.UtcNow.AddMinutes(15), signingCredentials: creds));

        var refreshToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        db.RefreshTokens.Add(new RefreshToken {
            UserId = user.Id, Token = refreshToken,
            ExpiresAt = DateTime.UtcNow.AddDays(7) });
        await db.SaveChangesAsync(ct);

        return new AuthResponse(accessToken, refreshToken);
    }
}
