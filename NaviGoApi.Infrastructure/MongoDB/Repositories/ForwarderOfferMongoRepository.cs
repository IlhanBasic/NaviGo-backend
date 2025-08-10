using MongoDB.Bson;
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
			offer.Id = await GetNextIdAsync();
			await _offersCollection.InsertOneAsync(offer);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _offersCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ForwarderOffers");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(ForwarderOffer offer)
		{
			await _offersCollection.DeleteOneAsync(o => o.Id == offer.Id);
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

		public async Task UpdateAsync(ForwarderOffer offer)
		{
			await _offersCollection.ReplaceOneAsync(o => o.Id == offer.Id, offer);
		}
	}
}
