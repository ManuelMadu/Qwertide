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

// Leaderboard. DESIGN-LAYER STUB: persists to browser localStorage so scores
// survive refresh (satisfies the "persistent" goal at the client level).
// M4/M5 swap this for the ASP.NET Core + EF Core API behind the same interface.
builder.Services.AddScoped<ILeaderboardService, LocalLeaderboardService>();

// Carries the just-finished run from Play -> Results without a querystring.
builder.Services.AddScoped<RunResultState>();

await builder.Build().RunAsync();
