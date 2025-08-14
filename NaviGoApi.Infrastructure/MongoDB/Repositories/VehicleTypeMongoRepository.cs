using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class VehicleTypeMongoRepository : IVehicleTypeRepository
	{
		private readonly IMongoCollection<VehicleType> _vehicleTypesCollection;

		public VehicleTypeMongoRepository(IMongoDatabase database)
		{
			_vehicleTypesCollection = database.GetCollection<VehicleType>("VehicleTypes");
		}

		public async Task AddAsync(VehicleType vehicleType)
		{
			vehicleType.Id = await GetNextIdAsync();
			await _vehicleTypesCollection.InsertOneAsync(vehicleType);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _vehicleTypesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "VehicleTypes");
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
			await _vehicleTypesCollection.DeleteOneAsync(vt => vt.Id == id);
		}

		public async Task<IEnumerable<VehicleType>> GetAllAsync()
		{
			return await _vehicleTypesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<VehicleType?> GetByIdAsync(int id)
		{
			return await _vehicleTypesCollection.Find(vt => vt.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(VehicleType vehicleType)
		{
			await _vehicleTypesCollection.ReplaceOneAsync(vt => vt.Id == vehicleType.Id, vehicleType);
		}

		public async Task<bool> ExistsAsync(Expression<Func<VehicleType, bool>> predicate)
		{
			var count = await _vehicleTypesCollection.CountDocumentsAsync(predicate);
			return count > 0;
		}

		public async Task<VehicleType?> GetByTypeName(string typeName)
		{
			return await _vehicleTypesCollection
				.Find(vt => vt.TypeName == typeName)
				.FirstOrDefaultAsync();
		}
	}
}
