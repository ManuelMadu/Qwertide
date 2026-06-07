# Deploying Qwertide to Azure App Service

Qwertide runs live at https://qwertide.azurewebsites.net as a single Linux
App Service. The ASP.NET Core API (`Qwertide.Api`) hosts both the leaderboard
endpoints and the published Blazor WebAssembly client, so there is one service
to deploy, one URL, and no CORS to configure in production.

This document records the exact deployment process, including the
subscription and region constraints that shaped it.

## What gets deployed

`dotnet publish` on the API project produces a self-contained bundle: the API
DLLs plus the compiled WASM client under `wwwroot`. That folder is zipped and
pushed to App Service with a zip deploy. The API serves the static client and
falls back to `index.html` for client-side routes (so a hard refresh on
`/leaderboard` works).

The leaderboard uses SQLite. The database file lives at `/home/qwertide.db`,
which is on the persistent `/home` share, so scores survive restarts and
redeploys.

## Prerequisites

- .NET 8 SDK (for `dotnet publish`).
- Azure CLI 2.87.0 or later (`az version`).
- An Azure subscription you have write access to (see the note below).
- `zip` available on the path.

## Subscription and region constraints (read first)

These two points cost time the first time around and are specific to this
account setup:

1. **Use the right subscription.** The default university subscription
   `sub-uoh-prod` is locked: student accounts have no write access and any
   resource creation fails with `AuthorizationFailed` on `resourcegroups/write`.
   Deploy to the personal "Azure for Students" subscription instead and set it
   active before doing anything else:

   ```bash
   az account set --subscription a7687e54-ee06-4308-8d89-e1a569f9b8d4
   ```

2. **Region policy.** The Students subscription only permits these regions:
   `swedencentral`, `spaincentral`, `italynorth`, `uaenorth`, `polandcentral`.
   `westeurope` and `northeurope` are blocked and return
   `RequestDisallowedByAzure`. This deployment uses `swedencentral` for the
   App Service plan and web app. (The resource group itself is metadata only,
   so its location does not matter.)

A harmless `SyntaxWarning: invalid escape sequence` line may print from the
Azure CLI. It is not an error and can be ignored.

## One-time setup

Resource names used here: resource group `qwertide-rg`, plan `qwertide-plan`,
web app `qwertide`.

```bash
# 1. Resource group
az group create --name qwertide-rg --location swedencentral

# 2. App Service plan, Free (F1) tier, Linux
az appservice plan create --name qwertide-plan --resource-group qwertide-rg \
  --sku F1 --is-linux --location swedencentral

# 3. Web app on the .NET 8 runtime
az webapp create --name qwertide --resource-group qwertide-rg \
  --plan qwertide-plan --runtime "DOTNETCORE:8.0"

# 4. Connection string pointing at the persistent /home share
az webapp config connection-string set --name qwertide \
  --resource-group qwertide-rg --connection-string-type Custom \
  --settings Qwertide="Data Source=/home/qwertide.db"

# 5. Force HTTPS
az webapp update --name qwertide --resource-group qwertide-rg --https-only true
```

The F1 (Free) tier costs nothing but sleeps when idle, so the first request
after a quiet period is slow. That cold start is expected.

## Publish and deploy

Run these from the repository root.

```bash
# Build the single-service bundle (API + bundled WASM client)
dotnet publish src/Qwertide.Api/Qwertide.Api.csproj -c Release -o ./publish

# Zip the contents of publish (not the folder itself) so it unpacks at the root
cd publish && zip -r ../app.zip .

# Deploy the zip (use an absolute path to app.zip to avoid shell cwd surprises)
az webapp deploy --name qwertide --resource-group qwertide-rg \
  --src-path app.zip --type zip
```

A successful deploy reports `Build successful`, then
`Site started successfully`, and finishes with `RuntimeSuccessful`.

The `publish/` folder and `app.zip` are both git-ignored so build artifacts
never get committed.

## Smoke test

```bash
open https://qwertide.azurewebsites.net
```

Confirm:

- The game loads at `/` and a run can be played and saved.
- `/leaderboard` shows the saved score alongside the seeded entries.
- A hard refresh directly on `/leaderboard` reloads the page (SPA fallback)
  rather than returning a 404.
- `https://qwertide.azurewebsites.net/api/scores?top=10` returns JSON.

## Redeploying

After code changes, repeat the three publish-and-deploy commands. The database
on `/home` is untouched, so existing scores persist across redeploys.

## Troubleshooting

Stream the live application logs:

```bash
az webapp log tail --name qwertide --resource-group qwertide-rg
```
