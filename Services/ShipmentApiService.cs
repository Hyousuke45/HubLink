using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Configuration;
using HubLink.Models;

namespace HubLink.Services;

public sealed class ShipmentApiService : IShipmentApiService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;

    public ShipmentApiService(HttpClient? httpClient = null)
    {
        _httpClient = httpClient ?? new HttpClient();
    }

    public async Task<ShipmentFetchResult> GetShipmentsAsync(
        ApiSettings settings,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(settings);

        if (string.IsNullOrWhiteSpace(settings.BaseUrl))
        {
            return ShipmentFetchResult.Failure("Service Base URL is not configured.");
        }

        if (string.IsNullOrWhiteSpace(settings.BranchCode))
        {
            return ShipmentFetchResult.Failure("Branch Code is not configured.");
        }

        Uri requestUri;
        try
        {
            requestUri = settings.BuildRequestUri();
        }
        catch (UriFormatException)
        {
            return ShipmentFetchResult.Failure($"Invalid service URL format: '{settings.BaseUrl}'.");
        }

        var timeoutSeconds = settings.TimeoutSeconds > 0 ? settings.TimeoutSeconds : 5;
        using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeoutCts.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

        HttpResponseMessage response;
        try
        {
            response = await _httpClient.GetAsync(requestUri, timeoutCts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return ShipmentFetchResult.Failure(
                $"Request timed out after {timeoutSeconds} second(s). The service took too long to respond.");
        }
        catch (HttpRequestException ex)
        {
            return ShipmentFetchResult.Failure(
                $"Unable to reach the HubLink service at {requestUri.Authority}. Check that the service is running and the address is correct. ({ex.Message})");
        }

        using (response)
        {
            var rawBody = await response.Content.ReadAsStringAsync(timeoutCts.Token);

            if (!response.IsSuccessStatusCode)
            {
                var apiError = TryExtractApiError(rawBody);
                var statusCode = (int)response.StatusCode;
                var detail = string.IsNullOrWhiteSpace(apiError)
                    ? response.ReasonPhrase ?? "Unsuccessful HTTP response."
                    : apiError;

                return ShipmentFetchResult.Failure($"Service returned HTTP {statusCode}: {detail}");
            }

            return ParseAndValidatePayload(rawBody, settings.BranchCode.Trim());
        }
    }

    private static string? TryExtractApiError(string rawBody)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return null;
        }

        try
        {
            var errorDto = JsonSerializer.Deserialize<ApiErrorDto>(rawBody, JsonOptions);
            return errorDto?.Error;
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static ShipmentFetchResult ParseAndValidatePayload(string rawBody, string fallbackBranchCode)
    {
        if (string.IsNullOrWhiteSpace(rawBody))
        {
            return ShipmentFetchResult.Failure("The service returned an empty response body instead of JSON.");
        }

        ShipmentApiResponseDto? dto;
        try
        {
            dto = JsonSerializer.Deserialize<ShipmentApiResponseDto>(rawBody, JsonOptions);
        }
        catch (JsonException ex)
        {
            return ShipmentFetchResult.Failure(
                $"Received unusable or malformed JSON from the service. ({ex.Message})");
        }

        if (dto is null || dto.Shipments is null)
        {
            return ShipmentFetchResult.Failure(
                "The JSON response did not contain the expected 'shipments' collection.");
        }

        var shipments = new List<Shipment>(dto.Shipments.Count);
        foreach (var item in dto.Shipments)
        {
            if (item is null ||
                string.IsNullOrWhiteSpace(item.TrackingNumber) ||
                string.IsNullOrWhiteSpace(item.Origin) ||
                string.IsNullOrWhiteSpace(item.Destination) ||
                string.IsNullOrWhiteSpace(item.Status) ||
                string.IsNullOrWhiteSpace(item.LastUpdatedUtc))
            {
                return ShipmentFetchResult.Failure(
                    "One or more shipment records in the response were missing required fields.");
            }

            if (!Enum.TryParse<ShipmentStatus>(item.Status.Trim(), ignoreCase: false, out var parsedStatus))
            {
                return ShipmentFetchResult.Failure(
                    $"Shipment '{item.TrackingNumber}' contained an unrecognized status '{item.Status}'.");
            }

            if (!DateTimeOffset.TryParse(
                    item.LastUpdatedUtc.Trim(),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                    out var parsedTimestamp))
            {
                return ShipmentFetchResult.Failure(
                    $"Shipment '{item.TrackingNumber}' contained an invalid UTC timestamp '{item.LastUpdatedUtc}'.");
            }

            shipments.Add(new Shipment
            {
                TrackingNumber = item.TrackingNumber.Trim(),
                BranchCode = string.IsNullOrWhiteSpace(item.BranchCode)
                    ? (dto.BranchCode ?? fallbackBranchCode)
                    : item.BranchCode.Trim(),
                Origin = item.Origin.Trim(),
                Destination = item.Destination.Trim(),
                Status = parsedStatus,
                LastUpdatedUtc = parsedTimestamp,
                RawLastUpdatedUtc = item.LastUpdatedUtc.Trim()
            });
        }

        var resolvedBranch = string.IsNullOrWhiteSpace(dto.BranchCode)
            ? fallbackBranchCode
            : dto.BranchCode.Trim();

        return ShipmentFetchResult.Success(resolvedBranch, shipments);
    }
}