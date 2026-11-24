using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using HubLink.Models;

namespace HubLink.Services;

public interface ICsvExportService
{
    string BuildCsvContent(IEnumerable<Shipment> shipments);

    Task ExportToFileAsync(
        string filePath,
        IEnumerable<Shipment> shipments,
        CancellationToken cancellationToken = default);
}