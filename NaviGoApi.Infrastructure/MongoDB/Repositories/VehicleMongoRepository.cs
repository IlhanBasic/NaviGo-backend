using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
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

		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync(int companyId)
		{
			return await _vehiclesCollection.Find(v => v.VehicleStatus == VehicleStatus.Free && v.CompanyId == companyId).ToListAsync();
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

		public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber)
		{
			return await _vehiclesCollection
				.Find(v => v.RegistrationNumber == registrationNumber)
				.FirstOrDefaultAsync();
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync(VehicleSearchDto vehicleSearch)
		{
			var filter = Builders<Vehicle>.Filter.Empty;

			if (!string.IsNullOrWhiteSpace(vehicleSearch.Brand))
			{
				filter &= Builders<Vehicle>.Filter.Regex(v => v.Brand, new BsonRegularExpression(vehicleSearch.Brand, "i"));
			}
			
			var sortBuilder = Builders<Vehicle>.Sort;
			var sort = vehicleSearch.SortDirection.ToLower() == "desc"
				? sortBuilder.Descending(vehicleSearch.SortBy ?? "Id")
				: sortBuilder.Ascending(vehicleSearch.SortBy ?? "Id");

			int skip = (vehicleSearch.Page - 1) * vehicleSearch.PageSize;

			var vehicles = await _vehiclesCollection
				.Find(filter)
				.Sort(sort)
				.Skip(skip)
				.Limit(vehicleSearch.PageSize)
				.ToListAsync();

			return vehicles;
		}

	}
}
