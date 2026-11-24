using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace HubLink.Models;

public sealed class ShipmentApiResponseDto
{
    [JsonPropertyName("branchCode")]
    public string? BranchCode { get; set; }

    [JsonPropertyName("shipments")]
    public List<ShipmentItemDto>? Shipments { get; set; }
}

public sealed class ShipmentItemDto
{
    [JsonPropertyName("trackingNumber")]
    public string? TrackingNumber { get; set; }

    [JsonPropertyName("branchCode")]
    public string? BranchCode { get; set; }

    [JsonPropertyName("origin")]
    public string? Origin { get; set; }

    [JsonPropertyName("destination")]
    public string? Destination { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("lastUpdatedUtc")]
    public string? LastUpdatedUtc { get; set; }
}

public sealed class ApiErrorDto
{
    [JsonPropertyName("error")]
    public string? Error { get; set; }
}