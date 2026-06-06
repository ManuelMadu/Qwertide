using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using Qwertide.Client;
using Qwertide.Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// MudBlazor providers (theme + snackbar).
builder.Services.AddMudServices(config =>
{
    config.SnackbarConfiguration.PositionClass = MudBlazor.Defaults.Classes.Position.BottomCenter;
    config.SnackbarConfiguration.ShowCloseIcon = false;
    config.SnackbarConfiguration.VisibleStateDuration = 2200;
    config.SnackbarConfiguration.HideTransitionDuration = 200;
});

// Passage source (static for v1; an /api/passages/random endpoint is a stretch goal).
builder.Services.AddSingleton<PassageLibrary>();

// Leaderboard talks to the ASP.NET Core + EF Core API (M5). The base URL is
// read from wwwroot/appsettings.json so dev and deployed builds point at the
// right host; LocalLeaderboardService remains as an offline localStorage fallback
// behind the same interface. A dedicated HttpClient is used because the API lives
// on a different origin than the WASM host.
// An empty/absent Api:BaseUrl means "same origin as the host" - the case when
// the API serves this client in production. Development config points it at the
// separately-running API (see wwwroot/appsettings.Development.json).
var apiBaseUrl = builder.Configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
}
builder.Services.AddScoped<ILeaderboardService>(_ =>
    new ApiLeaderboardService(new HttpClient { BaseAddress = new Uri(apiBaseUrl) }));

// Carries the just-finished run from Play -> Results without a querystring.
builder.Services.AddScoped<RunResultState>();

await builder.Build().RunAsync();
