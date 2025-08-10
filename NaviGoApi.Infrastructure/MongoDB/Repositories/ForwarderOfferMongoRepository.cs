using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ForwarderOfferMongoRepository : IForwarderOfferRepository
	{
		private readonly IMongoCollection<ForwarderOffer> _offersCollection;

		public ForwarderOfferMongoRepository(IMongoDatabase database)
		{
			_offersCollection = database.GetCollection<ForwarderOffer>("ForwarderOffers");
		}

		public async Task AddAsync(ForwarderOffer offer)
		{
			await _offersCollection.InsertOneAsync(offer);
		}

		public void Delete(ForwarderOffer offer)
		{
			_offersCollection.DeleteOne(o => o.Id == offer.Id);
		}

		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			var now = DateTime.UtcNow;
			return await _offersCollection
				.Find(o => o.ForwarderOfferStatus == ForwarderOfferStatus.Pending &&
						   o.ExpiresAt > now)
				.ToListAsync();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			return await _offersCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			return await _offersCollection
				.Find(o => o.ForwarderId == forwarderId)
				.ToListAsync();
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			return await _offersCollection
				.Find(o => o.Id == id)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			return await _offersCollection
				.Find(o => o.RouteId == routeId)
				.ToListAsync();
		}

		public void Update(ForwarderOffer offer)
		{
			_offersCollection.ReplaceOne(o => o.Id == offer.Id, offer);
		}
	}
}
