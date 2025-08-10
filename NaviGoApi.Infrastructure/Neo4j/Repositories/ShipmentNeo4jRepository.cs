using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentNeo4jRepository : IShipmentRepository
	{
		private readonly IDriver _driver;

		public ShipmentNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Shipment shipment)
		{
			var id = await GetNextIdAsync("Shipment");

			var query = @"
            CREATE (s:Shipment {
                id: $id,
                contractId: $contractId,
                vehicleId: $vehicleId,
                driverId: $driverId,
                cargoTypeId: $cargoTypeId,
                weightKg: $weightKg,
                priority: $priority,
                description: $description,
                status: $status,
                scheduledDeparture: datetime($scheduledDeparture),
                scheduledArrival: datetime($scheduledArrival),
                actualDeparture: $actualDeparture,
                actualArrival: $actualArrival,
                createdAt: datetime($createdAt),
                delayHours: $delayHours,
                delayPenaltyAmount: $delayPenaltyAmount,
                penaltyCalculatedAt: $penaltyCalculatedAt
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					contractId = shipment.ContractId,
					vehicleId = shipment.VehicleId,
					driverId = shipment.DriverId,
					cargoTypeId = shipment.CargoTypeId,
					weightKg = shipment.WeightKg,
					priority = shipment.Priority,
					description = shipment.Description,
					status = (int)shipment.Status,
					scheduledDeparture = shipment.ScheduledDeparture.ToString("o"),
					scheduledArrival = shipment.ScheduledArrival.ToString("o"),
					actualDeparture = shipment.ActualDeparture?.ToString("o"),
					actualArrival = shipment.ActualArrival?.ToString("o"),
					createdAt = shipment.CreatedAt.ToString("o"),
					delayHours = shipment.DelayHours,
					delayPenaltyAmount = shipment.DelayPenaltyAmount,
					penaltyCalculatedAt = shipment.PenaltyCalculatedAt?.ToString("o")
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Shipment shipment)
		{
			var query = "MATCH (s:Shipment {id: $id}) DETACH DELETE s";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { id = shipment.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			var query = "MATCH (s:Shipment) RETURN s";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query);
				var result = new List<Shipment>();

				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["s"].As<INode>();
					result.Add(MapNodeToShipment(node));
				}

				return result;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			var query = "MATCH (s:Shipment {contractId: $contractId}) RETURN s";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { contractId });
				var result = new List<Shipment>();

				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["s"].As<INode>();
					result.Add(MapNodeToShipment(node));
				}

				return result;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			var query = "MATCH (s:Shipment {id: $id}) RETURN s LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });

				if (await cursor.FetchAsync())
				{
					var node = cursor.Current["s"].As<INode>();
					return MapNodeToShipment(node);
				}
				return null;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			var query = "MATCH (s:Shipment {status: $status}) RETURN s";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { status = (int)status });
				var result = new List<Shipment>();

				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["s"].As<INode>();
					result.Add(MapNodeToShipment(node));
				}

				return result;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Shipment shipment)
		{
			var query = @"
                MATCH (s:Shipment {id: $id})
                SET s.contractId = $contractId,
                    s.vehicleId = $vehicleId,
                    s.driverId = $driverId,
                    s.cargoTypeId = $cargoTypeId,
                    s.weightKg = $weightKg,
                    s.priority = $priority,
                    s.description = $description,
                    s.status = $status,
                    s.scheduledDeparture = datetime($scheduledDeparture),
                    s.scheduledArrival = datetime($scheduledArrival),
                    s.actualDeparture = $actualDeparture,
                    s.actualArrival = $actualArrival,
                    s.createdAt = datetime($createdAt),
                    s.delayHours = $delayHours,
                    s.delayPenaltyAmount = $delayPenaltyAmount,
                    s.penaltyCalculatedAt = $penaltyCalculatedAt";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = shipment.Id,
					contractId = shipment.ContractId,
					vehicleId = shipment.VehicleId,
					driverId = shipment.DriverId,
					cargoTypeId = shipment.CargoTypeId,
					weightKg = shipment.WeightKg,
					priority = shipment.Priority,
					description = shipment.Description,
					status = (int)shipment.Status,
					scheduledDeparture = shipment.ScheduledDeparture.ToString("o"),
					scheduledArrival = shipment.ScheduledArrival.ToString("o"),
					actualDeparture = shipment.ActualDeparture?.ToString("o"),
					actualArrival = shipment.ActualArrival?.ToString("o"),
					createdAt = shipment.CreatedAt.ToString("o"),
					delayHours = shipment.DelayHours,
					delayPenaltyAmount = shipment.DelayPenaltyAmount,
					penaltyCalculatedAt = shipment.PenaltyCalculatedAt?.ToString("o")
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private Shipment MapNodeToShipment(INode node)
		{
			return new Shipment
			{
				Id = Convert.ToInt32(node.Properties["id"]),
				ContractId = Convert.ToInt32(node.Properties["contractId"]),
				VehicleId = Convert.ToInt32(node.Properties["vehicleId"]),
				DriverId = Convert.ToInt32(node.Properties["driverId"]),
				CargoTypeId = Convert.ToInt32(node.Properties["cargoTypeId"]),
				WeightKg = Convert.ToDouble(node.Properties["weightKg"]),
				Priority = Convert.ToInt32(node.Properties["priority"]),
				Description = node.Properties.ContainsKey("description") ? node.Properties["description"]?.ToString() : null,
				Status = (ShipmentStatus)Convert.ToInt32(node.Properties["status"]),
				ScheduledDeparture = DateTime.Parse(node.Properties["scheduledDeparture"].ToString()),
				ScheduledArrival = DateTime.Parse(node.Properties["scheduledArrival"].ToString()),
				ActualDeparture = node.Properties.ContainsKey("actualDeparture") && node.Properties["actualDeparture"] != null
					? (DateTime?)DateTime.Parse(node.Properties["actualDeparture"].ToString())
					: null,
				ActualArrival = node.Properties.ContainsKey("actualArrival") && node.Properties["actualArrival"] != null
					? (DateTime?)DateTime.Parse(node.Properties["actualArrival"].ToString())
					: null,
				CreatedAt = DateTime.Parse(node.Properties["createdAt"].ToString()),
				DelayHours = node.Properties.ContainsKey("delayHours") && node.Properties["delayHours"] != null
					? (int?)Convert.ToInt32(node.Properties["delayHours"])
					: null,
				DelayPenaltyAmount = node.Properties.ContainsKey("delayPenaltyAmount") && node.Properties["delayPenaltyAmount"] != null
					? (decimal?)Convert.ToDecimal(node.Properties["delayPenaltyAmount"])
					: null,
				PenaltyCalculatedAt = node.Properties.ContainsKey("penaltyCalculatedAt") && node.Properties["penaltyCalculatedAt"] != null
					? (DateTime?)DateTime.Parse(node.Properties["penaltyCalculatedAt"].ToString())
					: null
			};
		}
	}
}
