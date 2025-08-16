
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Common.DTOs;

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
                MATCH (v:Vehicle { id: $VehicleId })
                MATCH (u:User { Id: $ReportedByUserId })
                CREATE (vm:VehicleMaintenance {
                    Id: $Id,
                    Description: $Description,
                    ReportedAt: datetime($ReportedAt),
                    ResolvedAt: CASE WHEN $ResolvedAt IS NULL THEN NULL ELSE datetime($ResolvedAt) END,
                    Severity: $Severity,
                    RepairCost: $RepairCost,
                    MaintenanceType: $MaintenanceType
                })
                MERGE (vm)-[:FOR_VEHICLE]->(v)
                MERGE (vm)-[:REPORTED_BY]->(u)
            ";
			Console.WriteLine($"VehicleId: {maintenance.VehicleId}, ReportedByUserId: {maintenance.ReportedByUserId}");

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					VehicleId = maintenance.VehicleId,
					ReportedByUserId = maintenance.ReportedByUserId,
					Description = maintenance.Description,
					ReportedAt = maintenance.ReportedAt,
					ResolvedAt = maintenance.ResolvedAt,
					Severity = (int)maintenance.Severity,
					RepairCost = maintenance.RepairCost,
					MaintenanceType = (int)maintenance.MaintenanceType
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"
                MATCH (vm:VehicleMaintenance { Id: $Id })
                DETACH DELETE vm
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { Id = id });
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
        OPTIONAL MATCH (vm)-[:FOR_VEHICLE]->(v:Vehicle)
        OPTIONAL MATCH (vm)-[:REPORTED_BY]->(u:User)
        RETURN vm, v, u
    ";

			var session = _driver.AsyncSession();
			var list = new List<VehicleMaintenance>();
			try
			{
				var result = await session.RunAsync(query);
				await result.ForEachAsync(record =>
				{
					var vmNode = record["vm"].As<INode>();
					var vehicleNode = record["v"]?.As<INode>();
					var userNode = record["u"]?.As<INode>();

					DateTime ConvertDate(object val)
					{
						if (val is ZonedDateTime zdt)
							return zdt.ToDateTimeOffset().UtcDateTime;
						if (val is DateTime dt)
							return dt;
						throw new InvalidCastException("Unknown date type");
					}

					var maintenance = new VehicleMaintenance
					{
						Id = vmNode["Id"].As<int>(),
						VehicleId = vehicleNode?.Properties.ContainsKey("id") == true ? vehicleNode["id"].As<int>() : 0,
						ReportedByUserId = userNode?.Properties.ContainsKey("id") == true ? userNode["id"].As<int>() : 0,
						Description = vmNode["Description"].As<string>(),
						ReportedAt = ConvertDate(vmNode["ReportedAt"]),
						ResolvedAt = vmNode.Properties.ContainsKey("ResolvedAt") ? ConvertDate(vmNode["ResolvedAt"]) : null,
						Severity = (Domain.Entities.Severity)vmNode["Severity"].As<int>(),
						RepairCost = vmNode.Properties.ContainsKey("RepairCost") ? vmNode["RepairCost"].As<decimal?>() : null,
						MaintenanceType = (MaintenanceType)vmNode["MaintenanceType"].As<int>(),
						Vehicle = vehicleNode != null ? new Vehicle
						{
							Id = vehicleNode.Properties.ContainsKey("id") ? vehicleNode["id"].As<int>() : 0,
							Brand = vehicleNode.Properties.ContainsKey("brand") ? vehicleNode["brand"].As<string>() : null,
							Model = vehicleNode.Properties.ContainsKey("model") ? vehicleNode["model"].As<string>() : null
						} : null,
						ReportedByUser = userNode != null ? new User
						{
							Id = userNode.Properties.ContainsKey("id") ? userNode["id"].As<int>() : 0,
							Email = userNode.Properties.ContainsKey("email") ? userNode["email"].As<string>() : null
						} : null
					};
					list.Add(maintenance);
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
			var query = @"
        MATCH (vm:VehicleMaintenance { Id: $Id })
        OPTIONAL MATCH (vm)-[:FOR_VEHICLE]->(v:Vehicle)
        OPTIONAL MATCH (vm)-[:REPORTED_BY]->(u:User)
        RETURN vm, v, u
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var records = await result.ToListAsync();

				if (records.Count == 0)
					return null;

				var record = records[0];
				var vmNode = record["vm"].As<INode>();
				var vehicleNode = record["v"]?.As<INode>();
				var userNode = record["u"]?.As<INode>();

				DateTime ConvertDate(object val)
				{
					if (val is ZonedDateTime zdt)
						return zdt.ToDateTimeOffset().UtcDateTime;
					if (val is DateTime dt)
						return dt;
					throw new InvalidCastException("Unknown date type");
				}

				return new VehicleMaintenance
				{
					Id = vmNode["Id"].As<int>(),
					VehicleId = vehicleNode?.Properties.ContainsKey("id") == true ? vehicleNode["id"].As<int>() : 0,
					ReportedByUserId = userNode?.Properties.ContainsKey("id") == true ? userNode["id"].As<int>() : 0,
					Description = vmNode["Description"].As<string>(),
					ReportedAt = ConvertDate(vmNode["ReportedAt"]),
					ResolvedAt = vmNode.Properties.ContainsKey("ResolvedAt") && vmNode["ResolvedAt"] != null
						? ConvertDate(vmNode["ResolvedAt"])
						: (DateTime?)null,
					Severity = (Domain.Entities.Severity)vmNode["Severity"].As<int>(),
					RepairCost = vmNode.Properties.ContainsKey("RepairCost") && vmNode["RepairCost"] != null
						? vmNode["RepairCost"].As<decimal?>()
						: null,
					MaintenanceType = (MaintenanceType)vmNode["MaintenanceType"].As<int>(),
					Vehicle = vehicleNode != null ? new Vehicle
					{
						Id = vehicleNode.Properties.ContainsKey("id") ? vehicleNode["id"].As<int>() : 0,
						Brand = vehicleNode.Properties.ContainsKey("brand") ? vehicleNode["brand"].As<string>() : null,
						Model = vehicleNode.Properties.ContainsKey("model") ? vehicleNode["model"].As<string>() : null
					} : null,
					ReportedByUser = userNode != null ? new User
					{
						Id = userNode.Properties.ContainsKey("id") ? userNode["id"].As<int>() : 0,
						Email = userNode.Properties.ContainsKey("email") ? userNode["email"].As<string>() : null
					} : null
				};
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(VehicleMaintenance maintenance)
		{
			var query = @"
                MATCH (vm:VehicleMaintenance { Id: $Id })
                SET vm.Description = $Description,
                    vm.ReportedAt = datetime($ReportedAt),
                    vm.ResolvedAt = CASE WHEN $ResolvedAt IS NULL THEN NULL ELSE datetime($ResolvedAt) END,
                    vm.Severity = $Severity,
                    vm.RepairCost = $RepairCost,
                    vm.MaintenanceType = $MaintenanceType
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = maintenance.Id,
					Description = maintenance.Description,
					ReportedAt = maintenance.ReportedAt,
					ResolvedAt = maintenance.ResolvedAt,
					Severity = (int)maintenance.Severity,
					RepairCost = maintenance.RepairCost,
					MaintenanceType = (int)maintenance.MaintenanceType
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public Task<IEnumerable<VehicleMaintenance>> GetAllAsync(VehicleMaintenanceSearchDto vehicleMaintenanceSearch)
		{
			throw new NotImplementedException();
		}
	}
}
