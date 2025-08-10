using MongoDB.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.MongoDB.Repositories
{
	public class VehicleMaintenanceMongoRepository : IVehicleMaintenanceRepository
	{
		private readonly IMongoCollection<VehicleMaintenance> _vehicleMaintenancesCollection;

		public VehicleMaintenanceMongoRepository(IMongoDatabase database)
		{
			_vehicleMaintenancesCollection = database.GetCollection<VehicleMaintenance>("VehicleMaintenances");
		}

		public async Task AddAsync(VehicleMaintenance maintenance)
		{
			await _vehicleMaintenancesCollection.InsertOneAsync(maintenance);
		}

		public async Task DeleteAsync(int id)
		{
			await _vehicleMaintenancesCollection.DeleteOneAsync(vm => vm.Id == id);
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync()
		{
			return await _vehicleMaintenancesCollection.Find(_ => true).ToListAsync();
		}

		public async Task<VehicleMaintenance?> GetByIdAsync(int id)
		{
			return await _vehicleMaintenancesCollection.Find(vm => vm.Id == id).FirstOrDefaultAsync();
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			await _vehicleMaintenancesCollection.ReplaceOneAsync(vm => vm.Id == maintenance.Id, maintenance);
		}

		public Task SaveChangesAsync()
		{
			// MongoDB operacije su odmah sačuvane, pa nema potrebe za eksplicitnim SaveChanges.
			return Task.CompletedTask;
		}
	}
}
