using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class VehicleMongoRepository : IVehicleRepository
	{
		private readonly IMongoCollection<Vehicle> _vehiclesCollection;

		public VehicleMongoRepository(IMongoDatabase database)
		{
			_vehiclesCollection = database.GetCollection<Vehicle>("Vehicles");
		}

		public async Task AddAsync(Vehicle vehicle)
		{
			vehicle.Id = await GetNextIdAsync("Vehicles");
			await _vehiclesCollection.InsertOneAsync(vehicle);
		}

		private async Task<int> GetNextIdAsync(string collectionName)
		{
			var counters = _vehiclesCollection.Database.GetCollection<BsonDocument>("Counters");

			var filter = Builders<BsonDocument>.Filter.Eq("_id", collectionName);
			var update = Builders<BsonDocument>.Update.Inc("SequenceValue", 1);

			var options = new FindOneAndUpdateOptions<BsonDocument>
			{
				IsUpsert = true,
				ReturnDocument = ReturnDocument.After
			};

			var result = await counters.FindOneAndUpdateAsync(filter, update, options);
			return result["SequenceValue"].AsInt32;
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			return await _vehiclesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
		{
			return await _vehiclesCollection.Find(v => v.VehicleStatus == VehicleStatus.Free).ToListAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
		{
			return await _vehiclesCollection.Find(v => v.CompanyId == companyId).ToListAsync();
		}

		public async Task<Vehicle?> GetByIdAsync(int id)
		{
			return await _vehiclesCollection.Find(v => v.Id == id).FirstOrDefaultAsync();
		}

		public async Task DeleteAsync(Vehicle vehicle)
		{
			await _vehiclesCollection.DeleteOneAsync(v => v.Id == vehicle.Id);
		}

		public async Task UpdateAsync(Vehicle vehicle)
		{
			await _vehiclesCollection.ReplaceOneAsync(v => v.Id == vehicle.Id, vehicle);
		}
	}
}
