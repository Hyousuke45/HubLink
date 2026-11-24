# HubLink — Regional Logistics Data Exchange

Desktop client application built with C#, .NET, and Avalonia UI (MVVM) for retrieving, filtering, summarizing, and exporting partner branch shipment records from the HubLink REST API.

## Development Pair
- **Member 1:** [Gil Scudge Belonio Ybanez]
- **Member 2:** [Stephen Joseph Domingo Rubin]

## Prerequisites
- .NET 10.0 SDK (or compatible .NET SDK)
- HubLink Local API Service (`Start-HubLink.cmd`) or instructor-provided LAN service URL

## Configuration
Edit `appsettings.json` before running (or adjust fields directly in the application's top configuration bar):

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

## Acknowledgments & External Contributions
- **AI Assistance:** Google Gemini was consulted during development for architectural planning, Avalonia XAML layout structuring, edge-case error handling, and documentation formatting. All generated code and outputs were reviewed, integrated, tested against the live HubLink service, and verified by both development pair members.
- **Third-Party Libraries:** Built using Avalonia UI, `Avalonia.Controls.DataGrid`, `CommunityToolkit.Mvvm`, and `Microsoft.Extensions.Configuration`.