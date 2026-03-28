using System.Text;
using LogSage.Api.Data;
using LogSage.Api.Endpoints;
using LogSage.Api.Infrastructure;
using LogSage.Api.Middleware;
using LogSage.Api.Services;
using LogSage.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<LogSageEngine>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiAnalysisService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<StripeService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, ct) =>
    {
        document.Info = new()
        {
            Title = "LogSage API",
            Version = "v1",
            Description = "AI-powered log analysis API. Free tier available — no auth required for basic analysis.",
            Contact = new() { Email = "buildbysarmad@gmail.com" }
        };
        return Task.CompletedTask;
    });
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
});

builder.Services.AddCors(opt =>
    opt.AddPolicy("frontend", p =>
        p.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(','))
         .AllowAnyMethod().AllowAnyHeader().AllowCredentials()));

builder.Services.AddHealthChecks().AddDbContextCheck<AppDbContext>();

var app = builder.Build();

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseAuthentication();
app.UseAuthorization();
app.UseCors("frontend");

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.Title = "LogSage API";
        options.Theme = ScalarTheme.DeepSpace;
        options.DefaultHttpClient = new(ScalarTarget.CSharp, ScalarClient.HttpClient);
        options.Authentication = new ScalarAuthenticationOptions
        {
            PreferredSecurityScheme = "Bearer"
        };
    });

    app.MapPost("/api/dev/make-pro", async (
        string email, AppDbContext db, CancellationToken ct) =>
    {
        var user = await db.Users.FirstOrDefaultAsync(
            u => u.Email == email, ct);
        if (user == null) return Results.NotFound();
        user.Plan = "pro";
        await db.SaveChangesAsync(ct);
        return Results.Ok(new { message = $"{email} is now pro" });
    });
}

app.MapHealthChecks("/health");
app.MapAnalyzeEndpoints();
app.MapAuthEndpoints();
app.MapBillingEndpoints();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>()
        .Database.MigrateAsync();
}

app.Run();
