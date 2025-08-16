using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IForwarderOfferRepository
	{
		Task<ForwarderOffer?> GetByIdAsync(int id);
		Task<IEnumerable<ForwarderOffer>> GetAllAsync();
		Task<IEnumerable<ForwarderOffer>> GetAllAsync(ForwarderOfferSearchDto forwarderOfferSearch);

		Task AddAsync(ForwarderOffer offer);
		Task UpdateAsync(ForwarderOffer offer);
		Task DeleteAsync(ForwarderOffer offer);

		// Dodatne metode
		Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId);
		Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId);
		Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync();
	}
}
