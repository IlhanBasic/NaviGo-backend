using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

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
                    Id: $id,
                    ContractId: $contractId,
                    VehicleId: $vehicleId,
                    DriverId: $driverId,
                    CargoTypeId: $cargoTypeId,
                    WeightKg: $weightKg,
                    Priority: $priority,
                    Description: $description,
                    Status: $status,
                    ScheduledDeparture: $scheduledDeparture,
                    ScheduledArrival: $scheduledArrival,
                    ActualDeparture: $actualDeparture,
                    ActualArrival: $actualArrival,
                    CreatedAt: $createdAt
                })";

			var session = _driver.AsyncSession();
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
				scheduledDeparture = shipment.ScheduledDeparture,
				scheduledArrival = shipment.ScheduledArrival,
				actualDeparture = shipment.ActualDeparture,
				actualArrival = shipment.ActualArrival,
				createdAt = shipment.CreatedAt
			});
			await session.CloseAsync();
		}

		public async Task DeleteAsync(Shipment shipment)
		{
			var query = @"MATCH (s:Shipment {Id: $id}) DETACH DELETE s";
			var session = _driver.AsyncSession();
			await session.RunAsync(query, new { id = shipment.Id });
			await session.CloseAsync();
		}

		public async Task<IEnumerable<Shipment>> GetAllAsync()
		{
			var query = @"MATCH (s:Shipment) RETURN s";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query);

			var list = new List<Shipment>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToShipment(result.Current["s"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<IEnumerable<Shipment>> GetByContractIdAsync(int contractId)
		{
			var query = @"MATCH (s:Shipment {ContractId: $contractId}) RETURN s";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { contractId });

			var list = new List<Shipment>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToShipment(result.Current["s"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<IEnumerable<Shipment>> GetByStatusAsync(ShipmentStatus status)
		{
			var query = @"MATCH (s:Shipment {Status: $status}) RETURN s";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { status = (int)status });

			var list = new List<Shipment>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToShipment(result.Current["s"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<Shipment?> GetByIdAsync(int id)
		{
			var query = @"MATCH (s:Shipment {Id: $id}) RETURN s LIMIT 1";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { id });

			if (!await result.FetchAsync())
			{
				await session.CloseAsync();
				return null;
			}

			var node = result.Current["s"].As<INode>();
			await session.CloseAsync();
			return MapNodeToShipment(node);
		}

		public async Task UpdateAsync(Shipment shipment)
		{
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
                    s.ScheduledDeparture = $scheduledDeparture,
                    s.ScheduledArrival = $scheduledArrival,
                    s.ActualDeparture = $actualDeparture,
                    s.ActualArrival = $actualArrival,
                    s.CreatedAt = $createdAt";

			var session = _driver.AsyncSession();
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
				scheduledDeparture = shipment.ScheduledDeparture,
				scheduledArrival = shipment.ScheduledArrival,
				actualDeparture = shipment.ActualDeparture,
				actualArrival = shipment.ActualArrival,
				createdAt = shipment.CreatedAt
			});
			await session.CloseAsync();
		}

		private DateTime? ConvertNeo4jDate(object? value)
		{
			if (value == null) return null;

			return value switch
			{
				ZonedDateTime zdt => zdt.ToDateTimeOffset().UtcDateTime,
				LocalDateTime ldt => new DateTime(ldt.Year, ldt.Month, ldt.Day, ldt.Hour, ldt.Minute, ldt.Second, ldt.Nanosecond / 1_000_000, DateTimeKind.Utc),
				DateTime dt => dt,
				string s => DateTime.Parse(s),
				_ => throw new InvalidCastException($"Unexpected date type: {value.GetType()}")
			};
		}

		private Shipment MapNodeToShipment(INode node)
		{
			int GetInt(string key) => node.Properties.ContainsKey(key) && node[key] != null
				? Convert.ToInt32(node[key])
				: 0;

			int? GetNullableInt(string key) => node.Properties.ContainsKey(key) && node[key] != null
				? Convert.ToInt32(node[key])
				: (int?)null;

			double GetDouble(string key) => node.Properties.ContainsKey(key) && node[key] != null
				? Convert.ToDouble(node[key])
				: 0.0;

			ShipmentStatus GetStatus(string key) => node.Properties.ContainsKey(key) && node[key] != null
				? (ShipmentStatus)Convert.ToInt32(node[key])
				: ShipmentStatus.Scheduled;

			string? GetString(string key) => node.Properties.ContainsKey(key) && node[key] != null
				? node[key].ToString()
				: null;

			DateTime GetDateTime(string key) => ConvertNeo4jDate(node.Properties.GetValueOrDefault(key)) ?? DateTime.MinValue;

			DateTime? GetNullableDateTime(string key) => ConvertNeo4jDate(node.Properties.GetValueOrDefault(key));

			return new Shipment
			{
				Id = GetInt("Id"),
				ContractId = GetInt("ContractId"),
				VehicleId = GetNullableInt("VehicleId"),
				DriverId = GetNullableInt("DriverId"),
				CargoTypeId = GetInt("CargoTypeId"),
				WeightKg = GetDouble("WeightKg"),
				Priority = GetInt("Priority"),
				Description = GetString("Description"),
				Status = GetStatus("Status"),
				ScheduledDeparture = GetDateTime("ScheduledDeparture"),
				ScheduledArrival = GetDateTime("ScheduledArrival"),
				ActualDeparture = GetNullableDateTime("ActualDeparture"),
				ActualArrival = GetNullableDateTime("ActualArrival"),
				CreatedAt = GetDateTime("CreatedAt")
			};
		}


		public async Task<IEnumerable<Shipment>> GetAllAsync(ShipmentSearchDto search)
		{
			var query = @"MATCH (s:Shipment) RETURN s";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query);

			var list = new List<Shipment>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToShipment(result.Current["s"].As<INode>()));
			}
			await session.CloseAsync();

			list = (search.SortBy?.ToLower(), search.SortDirection.ToLower()) switch
			{
				("id", "asc") => list.OrderBy(s => s.Id).ToList(),
				("id", "desc") => list.OrderByDescending(s => s.Id).ToList(),
				("scheduleddeparture", "asc") => list.OrderBy(s => s.ScheduledDeparture).ToList(),
				("scheduleddeparture", "desc") => list.OrderByDescending(s => s.ScheduledDeparture).ToList(),
				("priority", "asc") => list.OrderBy(s => s.Priority).ToList(),
				("priority", "desc") => list.OrderByDescending(s => s.Priority).ToList(),
				("status", "asc") => list.OrderBy(s => s.Status).ToList(),
				("status", "desc") => list.OrderByDescending(s => s.Status).ToList(),
				_ => list.OrderBy(s => s.Id).ToList()
			};

			int skip = (search.Page - 1) * search.PageSize;
			list = list.Skip(skip).Take(search.PageSize).ToList();

			return list;
		}
	}
}
