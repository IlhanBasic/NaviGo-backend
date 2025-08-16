using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class ShipmentDocumentMongoRepository : IShipmentDocumentRepository
	{
		private readonly IMongoCollection<ShipmentDocument> _shipmentDocumentsCollection;

		public ShipmentDocumentMongoRepository(IMongoDatabase database)
		{
			_shipmentDocumentsCollection = database.GetCollection<ShipmentDocument>("ShipmentDocuments");
		}

		public async Task AddAsync(ShipmentDocument document)
		{
			document.Id = await GetNextIdAsync();
			await _shipmentDocumentsCollection.InsertOneAsync(document);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _shipmentDocumentsCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", "ShipmentDocuments");
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task DeleteAsync(int id)
		{
			await _shipmentDocumentsCollection.DeleteOneAsync(doc => doc.Id == id);
		}

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
		{
			return await _shipmentDocumentsCollection.Find(_ => true).ToListAsync();
		}

		public async Task<ShipmentDocument?> GetByIdAsync(int id)
		{
			return await _shipmentDocumentsCollection.Find(doc => doc.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(ShipmentDocument document)
		{
			await _shipmentDocumentsCollection.ReplaceOneAsync(doc => doc.Id == document.Id, document);
		}

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync(ShipmentDocumentSearchDto search)
		{
			var sortBuilder = Builders<ShipmentDocument>.Sort;

			// Odabir polja za sortiranje
			var sort = search.SortBy?.ToLower() switch
			{
				"uploaddate" => search.SortDirection.ToLower() == "desc"
					? sortBuilder.Descending(d => d.UploadDate)
					: sortBuilder.Ascending(d => d.UploadDate),
				"verified" => search.SortDirection.ToLower() == "desc"
					? sortBuilder.Descending(d => d.Verified)
					: sortBuilder.Ascending(d => d.Verified),
				_ => search.SortDirection.ToLower() == "desc"
					? sortBuilder.Descending(d => d.Id)
					: sortBuilder.Ascending(d => d.Id)
			};

			int skip = (search.Page - 1) * search.PageSize;

			var documents = await _shipmentDocumentsCollection
				.Find(_ => true)
				.Sort(sort)
				.Skip(skip)
				.Limit(search.PageSize)
				.ToListAsync();

			return documents;
		}

	}
}
