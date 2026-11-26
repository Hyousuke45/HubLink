# HubLink — Regional Logistics Data Exchange

Desktop client application built with C#, .NET 10, and Avalonia UI (MVVM) for retrieving, filtering, summarizing, and exporting partner branch shipment records from the HubLink REST API.

## Development Pair
- **Member 1:** Gil Scudge Ybanez
- **Member 2:** Stephen Joseph Rubin

## Prerequisites
- .NET 10.0 SDK
- HubLink Local API Service (`Start-HubLink.cmd`) or an instructor-provided LAN service URL

## Starting the Local API Service
1. Extract the instructor's local server package and double-click `Start-HubLink.cmd` (keep its console window open while using HubLink).
2. By default, the service listens on `http://127.0.0.1:5080`. If port `5080` is already in use, run `Start-HubLink-5081.cmd` instead and update `BaseUrl` to `http://127.0.0.1:5081`.

## Configuration
Edit `appsettings.json` in the project root before running (or adjust fields directly in the application's top configuration bar):

```json
{
  "HubLinkApi": {
    "BaseUrl": "[http://127.0.0.1:5080](http://127.0.0.1:5080)",
    "EndpointPath": "/api/shipments",
    "BranchCode": "BR-001",
    "Mode": "",
    "TimeoutSeconds": 5
  }
}
```

- **`BaseUrl`**: Base HTTP address of the HubLink API service (`http://127.0.0.1:5080` by default).
- **`EndpointPath`**: Normal retrieval endpoint (`/api/shipments`). Temporarily switch to `/api/test/shipments` when running controlled test scenarios, then click **Normal Endpoint** (and restore `/api/shipments`) afterward.
- **`BranchCode`**: Case-sensitive partner branch identifier (`BR-001`, `BR-002`, `BR-003`).
- **`Mode`**: Optional controlled test mode (`normal`, `empty`, `bad-request`, `server-error`, `invalid-json`, `delay`). Leave empty for normal retrieval.
- **`TimeoutSeconds`**: Client HTTP timeout in seconds (`5` seconds by default to catch the 10-second `delay` test mode).

## Restore, Build, and Run
Run the following commands from the directory containing `HubLink.csproj`:

```powershell
dotnet restore
dotnet build
dotnet run
```

## Acknowledgments & External Contributions
- **AI Assistance:** Google Gemini was consulted during development for architectural planning, Avalonia XAML layout structuring, edge-case error handling, and documentation formatting. All generated code and outputs were reviewed, integrated, tested against the live HubLink service, and verified by both development pair members.
- **Third-Party Libraries:** Built using Avalonia UI, `Avalonia.Controls.DataGrid`, `CommunityToolkit.Mvvm`, and `Microsoft.Extensions.Configuration`.