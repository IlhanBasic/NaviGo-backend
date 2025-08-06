using NaviGoApi.Domain.Entities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Interfaces
{
	public interface IForwarderOfferRepository
	{
		Task<ForwarderOffer?> GetByIdAsync(int id);
		Task<IEnumerable<ForwarderOffer>> GetAllAsync();
		Task AddAsync(ForwarderOffer offer);
		void Update(ForwarderOffer offer);
		void Delete(ForwarderOffer offer);

		// Dodatne metode
		Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId);
		Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId);
		Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync();
	}
}
