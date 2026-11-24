using System;
using System.Globalization;

namespace HubLink.Models;

public sealed class Shipment
{
    public required string TrackingNumber { get; init; }
    public required string BranchCode { get; init; }
    public required string Origin { get; init; }
    public required string Destination { get; init; }
    public required ShipmentStatus Status { get; init; }
    public required DateTimeOffset LastUpdatedUtc { get; init; }

    /// <summary>
    /// Preserves the exact ISO-8601 UTC timestamp string from the API for CSV export.
    /// </summary>
    public required string RawLastUpdatedUtc { get; init; }

    /// <summary>
    /// Human-readable UTC timestamp for display in the desktop UI.
    /// </summary>
    public string FormattedLastUpdatedUtc =>
        LastUpdatedUtc.ToUniversalTime().ToString("yyyy-MM-dd HH:mm:ss 'UTC'", CultureInfo.InvariantCulture);
}