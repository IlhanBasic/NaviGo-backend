//using MongoDB.Bson;
//using MongoDB.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;
//using System;
//using System.Collections.Generic;
//using System.ComponentModel.DataAnnotations;
//using System.Threading.Tasks;

//namespace NaviGoApi.Infrastructure.MongoDB.Repositories
//{
//	public class ForwarderOfferMongoRepository : IForwarderOfferRepository
//	{
//		private readonly IMongoCollection<ForwarderOffer> _offersCollection;

//		public ForwarderOfferMongoRepository(IMongoDatabase database)
//		{
//			_offersCollection = database.GetCollection<ForwarderOffer>("ForwarderOffers");
//		}

//		public async Task AddAsync(ForwarderOffer offer)
//		{
//			offer.Id = await GetNextIdAsync();
//			await _offersCollection.InsertOneAsync(offer);
//		}

//		private async Task<int> GetNextIdAsync()
//		{
//			var counters = _offersCollection.Database.GetCollection<BsonDocument>("Counters");
//			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ForwarderOffers");
//			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
//			var options = new FindOneAndUpdateOptions<BsonDocument>
//			{
//				IsUpsert = true,
//				ReturnDocument = ReturnDocument.After
//			};

//			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
//			return result["SequenceValue"].AsInt32;
//		}

//		public async Task DeleteAsync(ForwarderOffer offer)
//		{
//			var result = await _offersCollection.DeleteOneAsync(o => o.Id == offer.Id);
//			if (result.DeletedCount == 0)
//				throw new KeyNotFoundException($"ForwarderOffer with Id {offer.Id} not found for deletion.");
//		}

//		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
//		{
//			var offers = await _offersCollection.Find(_ => true).ToListAsync();
//			foreach (var offer in offers)
//				await LoadNavigationPropertiesAsync(offer);

//			return offers;
//		}

//		public async Task<ForwarderOffer?> GetByIdAsync(int id)
//		{
//			var offer = await _offersCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
//			if (offer != null)
//				await LoadNavigationPropertiesAsync(offer);
//			return offer;
//		}

//		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
//		{
//			var offers = await _offersCollection.Find(o => o.ForwarderId == forwarderId).ToListAsync();
//			foreach (var offer in offers)
//				await LoadNavigationPropertiesAsync(offer);
//			return offers;
//		}

//		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
//		{
//			var offers = await _offersCollection.Find(o => o.RouteId == routeId).ToListAsync();
//			foreach (var offer in offers)
//				await LoadNavigationPropertiesAsync(offer);
//			return offers;
//		}

//		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
//		{
//			var now = DateTime.UtcNow;
//			var offers = await _offersCollection.Find(o =>
//				o.ForwarderOfferStatus == ForwarderOfferStatus.Pending &&
//				o.ExpiresAt > now
//			).ToListAsync();

//			foreach (var offer in offers)
//				await LoadNavigationPropertiesAsync(offer);

//			return offers;
//		}

//		public async Task UpdateAsync(ForwarderOffer offer)
//		{
//			var result = await _offersCollection.ReplaceOneAsync(o => o.Id == offer.Id, offer);
//			if (result.MatchedCount == 0)
//				throw new ValidationException($"ForwarderOffer with Id {offer.Id} not found for update.");
//		}
//		private async Task LoadNavigationPropertiesAsync(ForwarderOffer offer)
//		{
//			if (offer.RouteId != 0)
//			{
//				var routesCollection = _offersCollection.Database.GetCollection<Route>("Routes");
//				offer.Route = await routesCollection.Find(r => r.Id == offer.RouteId).FirstOrDefaultAsync();
//			}

//			if (offer.ForwarderId != 0)
//			{
//				var companiesCollection = _offersCollection.Database.GetCollection<Company>("Companies");
//				offer.Forwarder = await companiesCollection.Find(c => c.Id == offer.ForwarderId).FirstOrDefaultAsync();
//			}
//		}
//	}
//}
using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ForwarderOfferMongoRepository : IForwarderOfferRepository
	{
		private readonly IMongoCollection<ForwarderOffer> _offersCollection;
		private readonly IMongoCollection<BsonDocument> _countersCollection;

		public ForwarderOfferMongoRepository(IMongoDatabase database)
		{
			_offersCollection = database.GetCollection<ForwarderOffer>("ForwarderOffers");
			_countersCollection = database.GetCollection<BsonDocument>("Counters");
		}

		public async Task AddAsync(ForwarderOffer offer)
		{
			offer.Id = await GetNextIdAsync();
			await _offersCollection.InsertOneAsync(offer);
		}

		private async Task<int> GetNextIdAsync()
		{
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ForwarderOffers");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);
			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await _countersCollection.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(ForwarderOffer offer)
		{
			var result = await _offersCollection.DeleteOneAsync(o => o.Id == offer.Id);
			if (result.DeletedCount == 0)
				throw new KeyNotFoundException($"ForwarderOffer with Id {offer.Id} not found for deletion.");
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			var offers = await _offersCollection.Find(_ => true).ToListAsync();
			return await PopulateRelationsAsync(offers);
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			var offer = await _offersCollection.Find(o => o.Id == id).FirstOrDefaultAsync();
			if (offer == null) return null;

			var populated = await PopulateRelationsAsync(new List<ForwarderOffer> { offer });
			return populated.FirstOrDefault();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			var offers = await _offersCollection.Find(o => o.ForwarderId == forwarderId).ToListAsync();
			return await PopulateRelationsAsync(offers);
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			var offers = await _offersCollection.Find(o => o.RouteId == routeId).ToListAsync();
			return await PopulateRelationsAsync(offers);
		}

		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			var now = DateTime.UtcNow;
			var offers = await _offersCollection.Find(o =>
				o.ForwarderOfferStatus == ForwarderOfferStatus.Pending &&
				o.ExpiresAt > now
			).ToListAsync();

			return await PopulateRelationsAsync(offers);
		}

		public async Task UpdateAsync(ForwarderOffer offer)
		{
			var result = await _offersCollection.ReplaceOneAsync(o => o.Id == offer.Id, offer);
			if (result.MatchedCount == 0)
				throw new ValidationException($"ForwarderOffer with Id {offer.Id} not found for update.");
		}

		private async Task<IEnumerable<ForwarderOffer>> PopulateRelationsAsync(IEnumerable<ForwarderOffer> offers)
		{
			var list = offers.ToList();
			if (!list.Any()) return list;

			var db = _offersCollection.Database;
			var routesCollection = db.GetCollection<Route>("Routes");
			var companiesCollection = db.GetCollection<Company>("Companies");

			foreach (var offer in list)
			{
				if (offer.RouteId != 0)
					offer.Route = await routesCollection.Find(r => r.Id == offer.RouteId).FirstOrDefaultAsync();

				if (offer.ForwarderId != 0)
					offer.Forwarder = await companiesCollection.Find(c => c.Id == offer.ForwarderId).FirstOrDefaultAsync();
			}

			return list;
		}
	}
}
