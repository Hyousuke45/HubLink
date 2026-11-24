using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

public sealed class CsvExportService : ICsvExportService
{
    private const string HeaderRow = "Tracking Number,Origin,Destination,Status,LastUpdatedUtc";

    public string BuildCsvContent(IEnumerable<Shipment> shipments)
    {
        ArgumentNullException.ThrowIfNull(shipments);

        var sb = new StringBuilder();
        sb.AppendLine(HeaderRow);

        foreach (var shipment in shipments)
        {
            var row = string.Join(",",
                EscapeCsvField(shipment.TrackingNumber),
                EscapeCsvField(shipment.Origin),
                EscapeCsvField(shipment.Destination),
                EscapeCsvField(shipment.Status.ToString()),
                EscapeCsvField(shipment.RawLastUpdatedUtc));

            sb.AppendLine(row);
        }

        return sb.ToString();
    }

    public async Task ExportToFileAsync(
        string filePath,
        IEnumerable<Shipment> shipments,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("A valid destination file path is required.", nameof(filePath));
        }

        ArgumentNullException.ThrowIfNull(shipments);

        var csvContent = BuildCsvContent(shipments);

        // Write using UTF-8 without BOM for clean cross-platform CSV compatibility
        var utf8NoBom = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false);
        await File.WriteAllTextAsync(filePath, csvContent, utf8NoBom, cancellationToken);
    }

    /// <summary>
    /// Escapes a field according to RFC 4180: wraps values containing commas,
    /// double quotes, or line breaks in double quotes and doubles internal quotes.
    /// </summary>
    public static string EscapeCsvField(string? value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return string.Empty;
        }

        var mustQuote = value.Contains(',') ||
                        value.Contains('"') ||
                        value.Contains('\r') ||
                        value.Contains('\n');

        if (!mustQuote)
        {
            return value;
        }

        var escapedQuotes = value.Replace("\"", "\"\"");
        return $"\"{escapedQuotes}\"";
    }
}