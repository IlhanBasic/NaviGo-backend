using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class VehicleMaintenanceNeo4jRepository : IVehicleMaintenanceRepository
	{
		private readonly IDriver _driver;

		public VehicleMaintenanceNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(VehicleMaintenance maintenance)
		{
			var query = @"
                CREATE (vm:VehicleMaintenance {
                    Id: $id,
                    VehicleId: $vehicleId,
                    ReportedByUserId: $reportedByUserId,
                    Description: $description,
                    ReportedAt: datetime($reportedAt),
                    ResolvedAt: $resolvedAt,
                    Severity: $severity,
                    RepairCost: $repairCost,
                    MaintenanceType: $maintenanceType
                })";

			var parameters = new Dictionary<string, object>
			{
				{ "id", maintenance.Id },
				{ "vehicleId", maintenance.VehicleId },
				{ "reportedByUserId", maintenance.ReportedByUserId },
				{ "description", maintenance.Description },
				{ "reportedAt", maintenance.ReportedAt.ToString("o") },
				{ "resolvedAt", maintenance.ResolvedAt?.ToString("o") },
				{ "severity", maintenance.Severity.ToString() },
				{ "repairCost", maintenance.RepairCost ?? 0m },
				{ "maintenanceType", maintenance.MaintenanceType.ToString() }
			};

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"
                MATCH (vm:VehicleMaintenance {Id: $id})
                DETACH DELETE vm";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<VehicleMaintenance>> GetAllAsync()
		{
			var query = @"
                MATCH (vm:VehicleMaintenance)
                RETURN vm";

			var session = _driver.AsyncSession();
			var results = new List<VehicleMaintenance>();
			try
			{
				var cursor = await session.RunAsync(query);
				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["vm"].As<INode>();
					results.Add(MapNodeToVehicleMaintenance(node));
				}
			}
			finally
			{
				await session.CloseAsync();
			}
			return results;
		}

		public async Task<VehicleMaintenance?> GetByIdAsync(int id)
		{
			var query = @"
                MATCH (vm:VehicleMaintenance {Id: $id})
                RETURN vm
                LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });
				if (await cursor.FetchAsync())
				{
					var node = cursor.Current["vm"].As<INode>();
					return MapNodeToVehicleMaintenance(node);
				}
				return null;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			var query = @"
                MATCH (vm:VehicleMaintenance {Id: $id})
                SET vm.VehicleId = $vehicleId,
                    vm.ReportedByUserId = $reportedByUserId,
                    vm.Description = $description,
                    vm.ReportedAt = datetime($reportedAt),
                    vm.ResolvedAt = $resolvedAt,
                    vm.Severity = $severity,
                    vm.RepairCost = $repairCost,
                    vm.MaintenanceType = $maintenanceType";

			var parameters = new Dictionary<string, object>
			{
				{ "id", maintenance.Id },
				{ "vehicleId", maintenance.VehicleId },
				{ "reportedByUserId", maintenance.ReportedByUserId },
				{ "description", maintenance.Description },
				{ "reportedAt", maintenance.ReportedAt.ToString("o") },
				{ "resolvedAt", maintenance.ResolvedAt?.ToString("o") },
				{ "severity", maintenance.Severity.ToString() },
				{ "repairCost", maintenance.RepairCost ?? 0m },
				{ "maintenanceType", maintenance.MaintenanceType.ToString() }
			};

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private VehicleMaintenance MapNodeToVehicleMaintenance(INode node)
		{
			return new VehicleMaintenance
			{
				Id = (int)(long)node.Properties["Id"],
				VehicleId = (int)(long)node.Properties["VehicleId"],
				ReportedByUserId = (int)(long)node.Properties["ReportedByUserId"],
				Description = node.Properties["Description"].As<string>(),
				ReportedAt = DateTime.Parse(node.Properties["ReportedAt"].As<string>()),
				ResolvedAt = node.Properties.ContainsKey("ResolvedAt") && node.Properties["ResolvedAt"] != null
					? DateTime.Parse(node.Properties["ResolvedAt"].As<string>())
					: (DateTime?)null,
				Severity = Enum.TryParse<Domain.Entities.Severity>(node.Properties["Severity"].As<string>(), out var sev) ? sev : Domain.Entities.Severity.Low,
				RepairCost = node.Properties.ContainsKey("RepairCost") ? (decimal?)(double)node.Properties["RepairCost"] : null,
				MaintenanceType = Enum.TryParse<MaintenanceType>(node.Properties["MaintenanceType"].As<string>(), out var mt) ? mt : MaintenanceType.Regular
			};
		}
	}
}
