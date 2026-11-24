using System;

namespace HubLink.Configuration;

public sealed class ApiSettings
{
    public string BaseUrl { get; set; } = "http://127.0.0.1:5080";
    public string EndpointPath { get; set; } = "/api/shipments";
    public string BranchCode { get; set; } = "BR-001";
    public string Mode { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 5;

    /// <summary>
    /// Builds the full request URI from the configured base URL, endpoint path,
    /// branchCode, and optional controlled test mode parameter.
    /// </summary>
    public Uri BuildRequestUri()
    {
        var trimmedBase = (BaseUrl ?? string.Empty).Trim().TrimEnd('/');
        var normalizedPath = (EndpointPath ?? "/api/shipments").Trim();

        if (!normalizedPath.StartsWith('/'))
        {
            normalizedPath = "/" + normalizedPath;
        }

        var query = $"?branchCode={Uri.EscapeDataString((BranchCode ?? string.Empty).Trim())}";

        if (!string.IsNullOrWhiteSpace(Mode))
        {
            query += $"&mode={Uri.EscapeDataString(Mode.Trim())}";
        }

        return new Uri($"{trimmedBase}{normalizedPath}{query}", UriKind.Absolute);
    }
}