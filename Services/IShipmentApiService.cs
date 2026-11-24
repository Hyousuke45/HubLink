using System.Threading;
using System.Threading.Tasks;
using HubLink.Configuration;
using HubLink.Models;

namespace HubLink.Services;

public interface IShipmentApiService
{
	Task<ShipmentFetchResult> GetShipmentsAsync(
		ApiSettings settings,
		CancellationToken cancellationToken = default);
}