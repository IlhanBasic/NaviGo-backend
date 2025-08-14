using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class VehicleNeo4jRepository : IVehicleRepository
	{
		private readonly IDriver _driver;

		public VehicleNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Vehicle vehicle)
		{
			var id = await GetNextIdAsync("Vehicle");

			var query = @"
            CREATE (v:Vehicle {
                id: $id,
                companyId: $companyId,
                brand: $brand,
                model: $model,
                engineCapacityCc: $engineCapacityCc,
                vehiclePicture: $vehiclePicture,
                vehicleTypeId: $vehicleTypeId,
                registrationNumber: $registrationNumber,
                capacityKg: $capacityKg,
                manufactureYear: $manufactureYear,
                vehicleStatus: $vehicleStatus,
                lastInspectionDate: datetime($lastInspectionDate),
                insuranceExpiry: datetime($insuranceExpiry),
                currentLocationId: $currentLocationId,
                createdAt: datetime($createdAt),
                categories: $categories
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					companyId=vehicle.CompanyId,
					brand=vehicle.Brand,
					model=vehicle.Model,
					engineCapacityCc=vehicle.EngineCapacityCc,
					vehiclePicture=vehicle.VehiclePicture ?? "",
					vehicleTypeId=vehicle.VehicleTypeId,
					registrationNumber=vehicle.RegistrationNumber,
					capacityKg=vehicle.CapacityKg,
					manufactureYear=vehicle.ManufactureYear,
					vehicleStatus = vehicle.VehicleStatus.ToString(),
					lastInspectionDate = vehicle.LastInspectionDate?.ToString("o"),
					insuranceExpiry = vehicle.InsuranceExpiry?.ToString("o"),
					currentLocationId=vehicle.CurrentLocationId,
					createdAt = vehicle.CreatedAt.ToString("o"),
					categories = vehicle.Categories ?? ""
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Vehicle vehicle)
		{
			var query = @"MATCH (v:Vehicle {id: $id}) DETACH DELETE v";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { id = vehicle.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Vehicle>> GetAllAsync()
		{
			var query = @"MATCH (v:Vehicle) RETURN v";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var list = new List<Vehicle>();

				await result.ForEachAsync(record =>
				{
					var node = record["v"].As<INode>();
					list.Add(NodeToEntity(node));
				});

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
		{
			var query = @"
				MATCH (v:Vehicle)
				WHERE v.vehicleStatus = $status
				RETURN v";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { status = VehicleStatus.Free.ToString() });
				var list = new List<Vehicle>();

				await result.ForEachAsync(record =>
				{
					var node = record["v"].As<INode>();
					list.Add(NodeToEntity(node));
				});

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
		{
			var query = @"
				MATCH (v:Vehicle)
				WHERE v.companyId = $companyId
				RETURN v";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { companyId });
				var list = new List<Vehicle>();

				await result.ForEachAsync(record =>
				{
					var node = record["v"].As<INode>();
					list.Add(NodeToEntity(node));
				});

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Vehicle?> GetByIdAsync(int id)
		{
			var query = @"MATCH (v:Vehicle {id: $id}) RETURN v LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { id });

				var found = await result.FetchAsync();
				if (!found) return null;

				var node = result.Current["v"].As<INode>();
				return NodeToEntity(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Vehicle vehicle)
		{
			var query = @"
				MATCH (v:Vehicle {id: $id})
				SET v.companyId = $companyId,
					v.brand = $brand,
					v.model = $model,
					v.engineCapacityCc = $engineCapacityCc,
					v.vehiclePicture = $vehiclePicture,
					v.vehicleTypeId = $vehicleTypeId,
					v.registrationNumber = $registrationNumber,
					v.capacityKg = $capacityKg,
					v.manufactureYear = $manufactureYear,
					v.vehicleStatus = $vehicleStatus,
					v.lastInspectionDate = datetime($lastInspectionDate),
					v.insuranceExpiry = datetime($insuranceExpiry),
					v.currentLocationId = $currentLocationId,
					v.categories = $categories";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = vehicle.Id,
					companyId = vehicle.CompanyId,
					brand = vehicle.Brand,
					model = vehicle.Model,
					engineCapacityCc = vehicle.EngineCapacityCc,
					vehiclePicture = vehicle.VehiclePicture ?? "",
					vehicleTypeId = vehicle.VehicleTypeId,
					registrationNumber = vehicle.RegistrationNumber,
					capacityKg = vehicle.CapacityKg,
					manufactureYear = vehicle.ManufactureYear,
					vehicleStatus = vehicle.VehicleStatus.ToString(),
					lastInspectionDate = vehicle.LastInspectionDate?.ToString("o"),
					insuranceExpiry = vehicle.InsuranceExpiry?.ToString("o"),
					currentLocationId = vehicle.CurrentLocationId,
					categories = vehicle.Categories ?? ""
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private Vehicle NodeToEntity(INode node)
		{
			return new Vehicle
			{
				Id = node.Properties.ContainsKey("id") ? Convert.ToInt32(node.Properties["id"]) : 0,
				CompanyId = node.Properties.ContainsKey("companyId") ? Convert.ToInt32(node.Properties["companyId"]) : 0,
				Brand = node.Properties.ContainsKey("brand") ? node.Properties["brand"].ToString()! : string.Empty,
				Model = node.Properties.ContainsKey("model") ? node.Properties["model"].ToString()! : string.Empty,
				EngineCapacityCc = node.Properties.ContainsKey("engineCapacityCc") ? Convert.ToInt32(node.Properties["engineCapacityCc"]) : 0,
				VehiclePicture = node.Properties.ContainsKey("vehiclePicture") ? node.Properties["vehiclePicture"].ToString() : "",
				VehicleTypeId = node.Properties.ContainsKey("vehicleTypeId") ? Convert.ToInt32(node.Properties["vehicleTypeId"]) : 0,
				RegistrationNumber = node.Properties.ContainsKey("registrationNumber") ? node.Properties["registrationNumber"].ToString()! : string.Empty,
				CapacityKg = node.Properties.ContainsKey("capacityKg") ? Convert.ToInt32(node.Properties["capacityKg"]) : 0,
				ManufactureYear = node.Properties.ContainsKey("manufactureYear") ? Convert.ToInt32(node.Properties["manufactureYear"]) : 0,
				VehicleStatus = node.Properties.ContainsKey("vehicleStatus") ? Enum.Parse<VehicleStatus>(node.Properties["vehicleStatus"].ToString()!) : VehicleStatus.Free,
				LastInspectionDate = node.Properties.ContainsKey("lastInspectionDate") && node.Properties["lastInspectionDate"] != null ? DateTime.Parse(node.Properties["lastInspectionDate"].ToString()!) : null,
				InsuranceExpiry = node.Properties.ContainsKey("insuranceExpiry") && node.Properties["insuranceExpiry"] != null ? DateTime.Parse(node.Properties["insuranceExpiry"].ToString()!) : null,
				CurrentLocationId = node.Properties.ContainsKey("currentLocationId") && node.Properties["currentLocationId"] != null ? (int?)Convert.ToInt32(node.Properties["currentLocationId"]) : null,
				Categories = node.Properties.ContainsKey("categories") ? node.Properties["categories"].ToString() : null
			};
		}

		public async Task<Vehicle?> GetByRegistrationNumberAsync(string registrationNumber)
		{
			var query = @"
        MATCH (v:Vehicle {registrationNumber: $registrationNumber})
        RETURN v
        LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { registrationNumber });

				var found = await result.FetchAsync();
				if (!found) return null;

				var node = result.Current["v"].As<INode>();
				return NodeToEntity(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

	}
}
