using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class VehicleMaintenanceNeo4jRepository : IVehicleMaintenanceRepository
	{
		private readonly IDriver _driver;

		public VehicleMaintenanceNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		private async Task<int> GetNextIdAsync(string entityName)
		{
			var query = @"
            MERGE (c:Counter { name: $entityName })
            ON CREATE SET c.lastId = 1
            ON MATCH SET c.lastId = c.lastId + 1
            RETURN c.lastId as lastId
        ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { entityName });
				var record = await result.SingleAsync();
				return record["lastId"].As<int>();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task AddAsync(VehicleMaintenance maintenance)
		{
			var id = await GetNextIdAsync("VehicleMaintenance");

			var query = @"
            CREATE (vm:VehicleMaintenance {
                id: $id,
                vehicleId: $vehicleId,
                reportedByUserId: $reportedByUserId,
                description: $description,
                reportedAt: datetime($reportedAt),
                resolvedAt: CASE WHEN $resolvedAt IS NULL THEN NULL ELSE datetime($resolvedAt) END,
                severity: $severity,
                repairCost: $repairCost,
                maintenanceType: $maintenanceType
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					maintenance.VehicleId,
					maintenance.ReportedByUserId,
					maintenance.Description,
					reportedAt = maintenance.ReportedAt.ToString("o"),
					resolvedAt = maintenance.ResolvedAt?.ToString("o"),
					severity = maintenance.Severity.ToString(),
					maintenance.RepairCost,
					maintenanceType = maintenance.MaintenanceType.ToString()
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"MATCH (vm:VehicleMaintenance {id: $id}) DETACH DELETE vm";

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
			var query = @"MATCH (vm:VehicleMaintenance) RETURN vm";

			var session = _driver.AsyncSession();
			var list = new List<VehicleMaintenance>();
			try
			{
				var result = await session.RunAsync(query);
				await result.ForEachAsync(record =>
				{
					var node = record["vm"].As<INode>();
					list.Add(NodeToEntity(node));
				});
				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<VehicleMaintenance?> GetByIdAsync(int id)
		{
			var query = @"MATCH (vm:VehicleMaintenance {id: $id}) RETURN vm LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { id });
				var found = await result.FetchAsync();
				if (!found) return null;

				var node = result.Current["vm"].As<INode>();
				return NodeToEntity(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			var query = @"
				MATCH (vm:VehicleMaintenance {id: $id})
				SET vm.vehicleId = $vehicleId,
					vm.reportedByUserId = $reportedByUserId,
					vm.description = $description,
					vm.reportedAt = datetime($reportedAt),
					vm.resolvedAt = CASE WHEN $resolvedAt IS NULL THEN NULL ELSE datetime($resolvedAt) END,
					vm.severity = $severity,
					vm.repairCost = $repairCost,
					vm.maintenanceType = $maintenanceType";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = maintenance.Id,
					vehicleId = maintenance.VehicleId,
					reportedByUserId = maintenance.ReportedByUserId,
					description = maintenance.Description,
					reportedAt = maintenance.ReportedAt.ToString("o"),
					resolvedAt = maintenance.ResolvedAt?.ToString("o"),
					severity = maintenance.Severity.ToString(),
					repairCost = maintenance.RepairCost,
					maintenanceType = maintenance.MaintenanceType.ToString()
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private VehicleMaintenance NodeToEntity(INode node)
		{
			return new VehicleMaintenance
			{
				Id = node.Properties.ContainsKey("id") ? Convert.ToInt32(node.Properties["id"]) : 0,
				VehicleId = node.Properties.ContainsKey("vehicleId") ? Convert.ToInt32(node.Properties["vehicleId"]) : 0,
				ReportedByUserId = node.Properties.ContainsKey("reportedByUserId") ? Convert.ToInt32(node.Properties["reportedByUserId"]) : 0,
				Description = node.Properties.ContainsKey("description") ? node.Properties["description"].ToString()! : string.Empty,
				ReportedAt = node.Properties.ContainsKey("reportedAt") ? DateTime.Parse(node.Properties["reportedAt"].ToString()!) : default,
				ResolvedAt = node.Properties.ContainsKey("resolvedAt") && node.Properties["resolvedAt"] != null
					? DateTime.Parse(node.Properties["resolvedAt"].ToString()!)
					: null,
				Severity = node.Properties.ContainsKey("severity")
					? Enum.Parse<Domain.Entities.Severity>(node.Properties["severity"].ToString()!)
					: Domain.Entities.Severity.Low,
				RepairCost = node.Properties.ContainsKey("repairCost") ? (decimal?)Convert.ToDecimal(node.Properties["repairCost"]) : null,
				MaintenanceType = node.Properties.ContainsKey("maintenanceType")
					? Enum.Parse<MaintenanceType>(node.Properties["maintenanceType"].ToString()!)
					: MaintenanceType.Regular
			};
		}
	}
}
