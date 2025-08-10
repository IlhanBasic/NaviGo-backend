using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;

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
			await _vehiclesCollection.InsertOneAsync(vehicle);
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
		public void Delete(Vehicle vehicle)
		{
			_vehiclesCollection.DeleteOneAsync(v => v.Id == vehicle.Id).GetAwaiter().GetResult();
		}

		public void Update(Vehicle vehicle)
		{
			_vehiclesCollection.ReplaceOneAsync(v => v.Id == vehicle.Id, vehicle).GetAwaiter().GetResult();
		}

	}
}
