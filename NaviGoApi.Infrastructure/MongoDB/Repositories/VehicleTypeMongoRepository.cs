using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
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
			await _vehicleTypesCollection.InsertOneAsync(vehicleType);
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
	}
}
