using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentNeo4jRepository
	{
		private readonly IDriver _driver;

		public ShipmentNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Shipment shipment)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (s:Shipment {
                    Id: $id,
                    ContractId: $contractId,
                    VehicleId: $vehicleId,
                    DriverId: $driverId,
                    CargoTypeId: $cargoTypeId,
                    WeightKg: $weightKg,
                    Priority: $priority,
                    Description: $description,
                    Status: $status,
                    ScheduledDeparture: datetime($scheduledDeparture),
                    ScheduledArrival: datetime($scheduledArrival),
                    ActualDeparture: $actualDeparture,
                    ActualArrival: $actualArrival,
                    CreatedAt: datetime($createdAt),
                    DelayHours: $delayHours,
                    DelayPenaltyAmount: $delayPenaltyAmount,
                    PenaltyCalculatedAt: $penaltyCalculatedAt
                })";

			var parameters = new Dictionary<string, object>
			{
				["id"] = shipment.Id,
				["contractId"] = shipment.ContractId,
				["vehicleId"] = shipment.VehicleId,
				["driverId"] = shipment.DriverId,
				["cargoTypeId"] = shipment.CargoTypeId,
				["weightKg"] = shipment.WeightKg,
				["priority"] = shipment.Priority,
				["description"] = shipment.Description ?? "",
				["status"] = (int)shipment.Status,
				["scheduledDeparture"] = shipment.ScheduledDeparture.ToString("o"),
				["scheduledArrival"] = shipment.ScheduledArrival.ToString("o"),
				["actualDeparture"] = shipment.ActualDeparture?.ToString("o"),
				["actualArrival"] = shipment.ActualArrival?.ToString("o"),
				["createdAt"] = shipment.CreatedAt.ToString("o"),
				["delayHours"] = shipment.DelayHours ?? 0,
				["delayPenaltyAmount"] = shipment.DelayPenaltyAmount ?? 0m,
				["penaltyCalculatedAt"] = shipment.PenaltyCalculatedAt?.ToString("o")
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task UpdateAsync(Shipment shipment)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (s:Shipment {Id: $id})
                SET s.ContractId = $contractId,
                    s.VehicleId = $vehicleId,
                    s.DriverId = $driverId,
                    s.CargoTypeId = $cargoTypeId,
                    s.WeightKg = $weightKg,
                    s.Priority = $priority,
                    s.Description = $description,
                    s.Status = $status,
                    s.ScheduledDeparture = datetime($scheduledDeparture),
                    s.ScheduledArrival = datetime($scheduledArrival),
                    s.ActualDeparture = $actualDeparture,
                    s.ActualArrival = $actualArrival,
                    s.CreatedAt = datetime($createdAt),
                    s.DelayHours = $delayHours,
                    s.DelayPenaltyAmount = $delayPenaltyAmount,
                    s.PenaltyCalculatedAt = $penaltyCalculatedAt";

			var parameters = new Dictionary<string, object>
			{
				["id"] = shipment.Id,
				["contractId"] = shipment.ContractId,
				["vehicleId"] = shipment.VehicleId,
				["driverId"] = shipment.DriverId,
				["cargoTypeId"] = shipment.CargoTypeId,
				["weightKg"] = shipment.WeightKg,
				["priority"] = shipment.Priority,
				["description"] = shipment.Description ?? "",
				["status"] = (int)shipment.Status,
				["scheduledDeparture"] = shipment.ScheduledDeparture.ToString("o"),
				["scheduledArrival"] = shipment.ScheduledArrival.ToString("o"),
				["actualDeparture"] = shipment.ActualDeparture?.ToString("o"),
				["actualArrival"] = shipment.ActualArrival?.ToString("o"),
				["createdAt"] = shipment.CreatedAt.ToString("o"),
				["delayHours"] = shipment.DelayHours ?? 0,
				["delayPenaltyAmount"] = shipment.DelayPenaltyAmount ?? 0m,
				["penaltyCalculatedAt"] = shipment.PenaltyCalculatedAt?.ToString("o")
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (s:Shipment {Id: $id}) DETACH DELETE s";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (s:Shipment {Id: $id})
                RETURN s
                LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				if (!await cursor.FetchAsync()) return null;

				var node = cursor.Current["s"].As<INode>();

				return new Shipment
				{
					Id = (int)(long)node.Properties["Id"],
					ContractId = (int)(long)node.Properties["ContractId"],
					VehicleId = (int)(long)node.Properties["VehicleId"],
					DriverId = (int)(long)node.Properties["DriverId"],
					CargoTypeId = (int)(long)node.Properties["CargoTypeId"],
					WeightKg = (double)node.Properties["WeightKg"],
					Priority = (int)(long)node.Properties["Priority"],
					Description = node.Properties.ContainsKey("Description") ? (string)node.Properties["Description"] : null,
					Status = (ShipmentStatus)(int)(long)node.Properties["Status"],
					ScheduledDeparture = DateTime.Parse((string)node.Properties["ScheduledDeparture"]),
					ScheduledArrival = DateTime.Parse((string)node.Properties["ScheduledArrival"]),
					ActualDeparture = node.Properties.ContainsKey("ActualDeparture") && node.Properties["ActualDeparture"] != null
						? DateTime.Parse((string)node.Properties["ActualDeparture"])
						: (DateTime?)null,
					ActualArrival = node.Properties.ContainsKey("ActualArrival") && node.Properties["ActualArrival"] != null
						? DateTime.Parse((string)node.Properties["ActualArrival"])
						: (DateTime?)null,
					CreatedAt = DateTime.Parse((string)node.Properties["CreatedAt"]),
					DelayHours = node.Properties.ContainsKey("DelayHours") ? (int?)(long)node.Properties["DelayHours"] : null,
					DelayPenaltyAmount = node.Properties.ContainsKey("DelayPenaltyAmount") ? (decimal?)(double)node.Properties["DelayPenaltyAmount"] : null,
					PenaltyCalculatedAt = node.Properties.ContainsKey("PenaltyCalculatedAt") && node.Properties["PenaltyCalculatedAt"] != null
						? DateTime.Parse((string)node.Properties["PenaltyCalculatedAt"])
						: (DateTime?)null
				};
			});
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (s:Shipment) RETURN s";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<Shipment>();

				foreach (var record in records)
				{
					var node = record["s"].As<INode>();
					list.Add(new Shipment
					{
						Id = (int)(long)node.Properties["Id"],
						ContractId = (int)(long)node.Properties["ContractId"],
						VehicleId = (int)(long)node.Properties["VehicleId"],
						DriverId = (int)(long)node.Properties["DriverId"],
						CargoTypeId = (int)(long)node.Properties["CargoTypeId"],
						WeightKg = (double)node.Properties["WeightKg"],
						Priority = (int)(long)node.Properties["Priority"],
						Description = node.Properties.ContainsKey("Description") ? (string)node.Properties["Description"] : null,
						Status = (ShipmentStatus)(int)(long)node.Properties["Status"],
						ScheduledDeparture = DateTime.Parse((string)node.Properties["ScheduledDeparture"]),
						ScheduledArrival = DateTime.Parse((string)node.Properties["ScheduledArrival"]),
						ActualDeparture = node.Properties.ContainsKey("ActualDeparture") && node.Properties["ActualDeparture"] != null
							? DateTime.Parse((string)node.Properties["ActualDeparture"])
							: (DateTime?)null,
						ActualArrival = node.Properties.ContainsKey("ActualArrival") && node.Properties["ActualArrival"] != null
							? DateTime.Parse((string)node.Properties["ActualArrival"])
							: (DateTime?)null,
						CreatedAt = DateTime.Parse((string)node.Properties["CreatedAt"]),
						DelayHours = node.Properties.ContainsKey("DelayHours") ? (int?)(long)node.Properties["DelayHours"] : null,
						DelayPenaltyAmount = node.Properties.ContainsKey("DelayPenaltyAmount") ? (decimal?)(double)node.Properties["DelayPenaltyAmount"] : null,
						PenaltyCalculatedAt = node.Properties.ContainsKey("PenaltyCalculatedAt") && node.Properties["PenaltyCalculatedAt"] != null
							? DateTime.Parse((string)node.Properties["PenaltyCalculatedAt"])
							: (DateTime?)null
					});
				}

				return list;
			});
		}
	}
}
