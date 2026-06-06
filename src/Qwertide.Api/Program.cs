using Microsoft.AspNetCore.HttpOverrides;
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

builder.Services.AddDbContext<QwertideDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("Qwertide")));

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

// Trust the X-Forwarded-* headers Azure App Service's load balancer sets, so
// the app sees the original HTTPS scheme instead of looping on redirect behind
// the TLS-terminating proxy.
var forwardedHeaders = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto,
};
forwardedHeaders.KnownNetworks.Clear();
forwardedHeaders.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedHeaders);

app.UseHttpsRedirection();

// Serve the Blazor WASM client this API is bundled with (single-service deploy).
app.UseBlazorFrameworkFiles();
app.UseStaticFiles();

app.UseCors(ClientCors);
app.UseAuthorization();

app.MapControllers();
// Any non-API route falls through to the SPA entry point so client-side
// routing (/play, /results, /leaderboard) works on a full-page load.
app.MapFallbackToFile("index.html");

app.Run();
