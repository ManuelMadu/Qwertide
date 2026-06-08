using System.Threading.RateLimiting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Qwertide.Api.Data;

var builder = WebApplication.CreateBuilder(args);

// Allow the Blazor WASM client (served from a different origin) to call the API.
const string ClientCors = "QwertideClient";
var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
    options.AddPolicy(ClientCors, policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Throttle score submissions per client IP so the public POST endpoint can't be
// scripted to flood the leaderboard. Partitioning by IP means one abuser is
// limited without locking everyone else out. Client IP comes from the forwarded
// headers processed below, so behind Azure's proxy this is the real caller.
const string SubmitRateLimit = "submit";
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.AddPolicy(SubmitRateLimit, httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 5,
                Window = TimeSpan.FromMinutes(1),
            }));
});

builder.Services.AddDbContext<QwertideDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Qwertide")));

// Liveness/readiness probe at /health that also verifies the database connection,
// so a deploy or platform health check fails fast if the data store is unreachable.
builder.Services.AddHealthChecks()
    .AddDbContextCheck<QwertideDbContext>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply any pending migrations on startup so a fresh clone or deploy comes up
// with a ready database without a manual `dotnet ef database update` step.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<QwertideDbContext>();
    db.Database.Migrate();
    DbSeeder.Seed(db);
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Baseline security response headers on every response (incl. static files):
// stop MIME-sniffing, deny framing (clickjacking), and don't leak the referrer.
app.Use(async (context, next) =>
{
    var headers = context.Response.Headers;
    headers["X-Content-Type-Options"] = "nosniff";
    headers["X-Frame-Options"] = "DENY";
    headers["Referrer-Policy"] = "no-referrer";
    await next();
});

if (app.Environment.IsProduction())
{
    // Behind Azure App Service the TLS-terminating proxy sets X-Forwarded-*; trust
    // them so the app sees the original HTTPS scheme (and real client IP) instead
    // of looping on redirect. App Service is the only ingress, so clearing the
    // known-proxy list is safe here - but only here, hence the production gate.
    var forwardedHeaders = new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
    };
    forwardedHeaders.KnownNetworks.Clear();
    forwardedHeaders.KnownProxies.Clear();
    app.UseForwardedHeaders(forwardedHeaders);

    // Tell browsers to stay on HTTPS for this host on future visits.
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serve the Blazor WASM client this API is bundled with (single-service deploy).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors(ClientCors);
app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();
// Unmatched API routes must return a real 404 instead of falling through to the
// SPA shell below, which would answer 200 with index.html and surface on the
// client as a confusing JSON parse error.
app.MapFallback("api/{**rest}", () => Results.NotFound());
// Any other (non-API) route falls through to the SPA entry point so client-side
// routing (/play, /results, /leaderboard) works on a full-page load.
app.MapFallbackToFile("index.html");

app.Run();
