using System.Text;
using System.Threading.RateLimiting;
using LogSage.Api.Data;
using LogSage.Api.Endpoints;
using LogSage.Api.Infrastructure;
using LogSage.Api.Middleware;
using LogSage.Api.Services;
using LogSage.Api.Services.Payments;
using LogSage.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

// Bootstrap logger — logs startup errors before full configuration
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel — enforce 2MB max request body size
builder.WebHost.ConfigureKestrel(options =>
{
    options.Limits.MaxRequestBodySize = 2 * 1024 * 1024; // 2MB
});

// Configure Serilog
builder.Host.UseSerilog((ctx, services, lc) => lc
    .ReadFrom.Configuration(ctx.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithThreadId()
    .Enrich.WithProcessId()
    .Enrich.WithProperty("Application", "LogSage.Api")
    .WriteTo.Console(outputTemplate:
        "[{Timestamp:HH:mm:ss} {Level:u3}] {SourceContext}: {Message:lj}{NewLine}{Exception}")
    .WriteTo.GrafanaLoki(
        ctx.Configuration["Grafana:LokiUrl"] ?? "",
        credentials: null,
        labels: new[]
        {
            new LokiLabel { Key = "app", Value = "logsage-api" },
            new LokiLabel { Key = "env", Value = ctx.HostingEnvironment.EnvironmentName },
            new LokiLabel { Key = "version", Value = "1.0.0" }
        },
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
);

builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseNpgsql(builder.Configuration.GetConnectionString("Default")));

builder.Services.AddSingleton<LogSageEngine>();
builder.Services.AddHttpClient();
builder.Services.AddScoped<AiAnalysisService>();
builder.Services.AddScoped<SessionService>();
builder.Services.AddScoped<IPaymentProvider, PaddlePaymentProvider>();
builder.Services.AddScoped<BillingService>();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt => {
        opt.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuer = true, ValidateAudience = true,
            ValidateLifetime = true, ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 }
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

// Rate limiting for auth endpoints (brute force prevention)
builder.Services.AddRateLimiter(opt =>
{
    opt.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
    {
        var path = ctx.Request.Path.ToString();
        if (path.StartsWith("/api/auth/"))
        {
            var identifier = ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(identifier, _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10,
                    Window = TimeSpan.FromMinutes(1),
                    QueueProcessingOrder = QueueProcessingOrder.OldestFirst,
                    QueueLimit = 0
                });
        }
        return RateLimitPartition.GetNoLimiter<string>("default");
    });
    opt.RejectionStatusCode = 429;
});

var app = builder.Build();

// Serilog request logging — logs HTTP requests with timing
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate =
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers.UserAgent);
        diagnosticContext.Set("UserId",
            httpContext.User.FindFirst("sub")?.Value ?? "anonymous");
    };
});

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseRateLimiter();
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
}

app.MapHealthChecks("/health");
app.MapAnalyzeEndpoints();
app.MapAuthEndpoints();

// Only register billing endpoints when pricing is enabled
if (app.Configuration.GetValue<bool>("PRICING_ENABLED"))
{
    app.MapBillingEndpoints();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    await scope.ServiceProvider.GetRequiredService<AppDbContext>()
        .Database.MigrateAsync();
}

app.Run();
