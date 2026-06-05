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

app.UseHttpsRedirection();
app.UseCors(ClientCors);
app.UseAuthorization();
app.MapControllers();

app.Run();
