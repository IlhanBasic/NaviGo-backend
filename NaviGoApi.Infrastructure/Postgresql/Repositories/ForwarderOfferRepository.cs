using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Postgresql.Repositories
{
	public class ForwarderOfferRepository : IForwarderOfferRepository
	{
		public Task AddAsync(ForwarderOffer offer)
		{
			throw new NotImplementedException();
		}

		public void Delete(ForwarderOffer offer)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			throw new NotImplementedException();
		}

		public Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			throw new NotImplementedException();
		}

		public Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			throw new NotImplementedException();
		}

		public void Update(ForwarderOffer offer)
		{
			throw new NotImplementedException();
		}
	}
}
