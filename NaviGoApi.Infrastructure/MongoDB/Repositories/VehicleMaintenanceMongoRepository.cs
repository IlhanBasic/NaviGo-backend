using MongoDB.Bson;
using MongoDB.Driver;
using NaviGoApi.Common.DTOs;
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
			maintenance.Id = await GetNextIdAsync();
			await _vehicleMaintenancesCollection.InsertOneAsync(maintenance);
		}

		private async Task<int> GetNextIdAsync()
		{
			var counters = _vehicleMaintenancesCollection.Database.GetCollection<BsonDocument>("Counters");
			var filter = Builders<BsonDocument>.Filter.Eq("_id", "VehicleMaintenances");
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
			await _vehicleMaintenancesCollection.DeleteOneAsync(vm => vm.Id == id);
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync()
		{
			var list = await _vehicleMaintenancesCollection.Find(_ => true).ToListAsync();
			foreach (var vm in list)
				await LoadNavigationPropertiesAsync(vm);

			return list;
		}

		public async Task<VehicleMaintenance?> GetByIdAsync(int id)
		{
			var maintenance = await _vehicleMaintenancesCollection.Find(vm => vm.Id == id).FirstOrDefaultAsync();
			if (maintenance != null)
				await LoadNavigationPropertiesAsync(maintenance);

			return maintenance;
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			await _vehicleMaintenancesCollection.ReplaceOneAsync(vm => vm.Id == maintenance.Id, maintenance);
		}

		// Helper metoda za učitavanje navigacionih property-ja
		private async Task LoadNavigationPropertiesAsync(VehicleMaintenance? maintenance)
		{
			if (maintenance == null) return;

			if (maintenance.VehicleId != 0)
			{
				var vehiclesCollection = _vehicleMaintenancesCollection.Database.GetCollection<Vehicle>("Vehicles");
				maintenance.Vehicle = await vehiclesCollection.Find(v => v.Id == maintenance.VehicleId).FirstOrDefaultAsync();
			}

			if (maintenance.ReportedByUserId != 0)
			{
				var usersCollection = _vehicleMaintenancesCollection.Database.GetCollection<User>("Users");
				maintenance.ReportedByUser = await usersCollection.Find(u => u.Id == maintenance.ReportedByUserId).FirstOrDefaultAsync();
			}
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync(VehicleMaintenanceSearchDto search)
		{
			var allowedSortFields = new Dictionary<string, string>
	{
		{ "Id", "Id" },
		{ "ReportedAt", "ReportedAt" },
		{ "ResolvedAt", "ResolvedAt" },
		{ "Severity", "Severity" }
	};

			var sortField = allowedSortFields.ContainsKey(search.SortBy ?? "") ? allowedSortFields[search.SortBy!] : "Id";
			var sortDefinition = search.SortDirection?.ToLower() == "desc"
				? Builders<VehicleMaintenance>.Sort.Descending(sortField)
				: Builders<VehicleMaintenance>.Sort.Ascending(sortField);

			var skip = (search.Page - 1) * search.PageSize;
			var list = await _vehicleMaintenancesCollection
				.Find(_ => true)
				.Sort(sortDefinition)
				.Skip(skip)
				.Limit(search.PageSize)
				.ToListAsync();
			foreach (var vm in list)
				await LoadNavigationPropertiesAsync(vm);

			return list;
		}

	}
}
