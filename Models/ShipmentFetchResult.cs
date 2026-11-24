using System;
using System.Collections.Generic;

namespace HubLink.Models;

public sealed class ShipmentFetchResult
{
    public bool IsSuccess { get; private init; }
    public string BranchCode { get; private init; } = string.Empty;
    public IReadOnlyList<Shipment> Shipments { get; private init; } = Array.Empty<Shipment>();
    public string? ErrorMessage { get; private init; }
    public DateTimeOffset RetrievedAtUtc { get; private init; }

    public bool IsEmpty => IsSuccess && Shipments.Count == 0;

    public static ShipmentFetchResult Success(string branchCode, IReadOnlyList<Shipment> shipments) =>
        new()
        {
            IsSuccess = true,
            BranchCode = branchCode,
            Shipments = shipments,
            RetrievedAtUtc = DateTimeOffset.UtcNow
        };

    public static ShipmentFetchResult Failure(string errorMessage) =>
        new()
        {
            IsSuccess = false,
            ErrorMessage = errorMessage,
            RetrievedAtUtc = DateTimeOffset.UtcNow
        };
}