using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class VehicleNeo4jRepository 
	{
		private readonly IDriver _driver;

		public VehicleNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Vehicle vehicle)
		{
			var query = @"
                CREATE (v:Vehicle {
                    Id: $id,
                    CompanyId: $companyId,
                    Brand: $brand,
                    Model: $model,
                    EngineCapacityCc: $engineCapacityCc,
                    VehicleTypeId: $vehicleTypeId,
                    RegistrationNumber: $registrationNumber,
                    CapacityKg: $capacityKg,
                    ManufactureYear: $manufactureYear,
                    VehicleStatus: $vehicleStatus,
                    LastInspectionDate: $lastInspectionDate,
                    InsuranceExpiry: $insuranceExpiry,
                    CurrentLocationId: $currentLocationId,
                    CreatedAt: $createdAt,
                    Categories: $categories
                })";

			var parameters = new Dictionary<string, object?>
			{
				{ "id", vehicle.Id },
				{ "companyId", vehicle.CompanyId },
				{ "brand", vehicle.Brand },
				{ "model", vehicle.Model },
				{ "engineCapacityCc", vehicle.EngineCapacityCc },
				{ "vehicleTypeId", vehicle.VehicleTypeId },
				{ "registrationNumber", vehicle.RegistrationNumber },
				{ "capacityKg", vehicle.CapacityKg },
				{ "manufactureYear", vehicle.ManufactureYear },
				{ "vehicleStatus", (int)vehicle.VehicleStatus },
				{ "lastInspectionDate", vehicle.LastInspectionDate?.ToString("o") },
				{ "insuranceExpiry", vehicle.InsuranceExpiry?.ToString("o") },
				{ "currentLocationId", vehicle.CurrentLocationId },
				{ "createdAt", vehicle.CreatedAt.ToString("o") },
				{ "categories", vehicle.Categories ?? "" }
			};

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		// Implementacija Delete po interfejsu, prima Vehicle, a ti si koristio int id - ispravljeno:
		public async void Delete(Vehicle vehicle)
		{
			var query = @"
                MATCH (v:Vehicle {Id: $id})
                DETACH DELETE v";

			await using var session = _driver.AsyncSession();
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
			var vehicles = new List<Vehicle>();

			await using var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query);
				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["v"].As<INode>();
					vehicles.Add(MapNodeToVehicle(node));
				}
			}
			finally
			{
				await session.CloseAsync();
			}
			return vehicles;
		}

		public async Task<IEnumerable<Vehicle>> GetAvailableVehiclesAsync()
		{
			var query = @"
                MATCH (v:Vehicle)
                WHERE v.VehicleStatus = $status
                RETURN v";

			var vehicles = new List<Vehicle>();

			await using var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { status = (int)VehicleStatus.Free });
				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["v"].As<INode>();
					vehicles.Add(MapNodeToVehicle(node));
				}
			}
			finally
			{
				await session.CloseAsync();
			}
			return vehicles;
		}

		public async Task<IEnumerable<Vehicle>> GetByCompanyIdAsync(int companyId)
		{
			var query = @"
                MATCH (v:Vehicle {CompanyId: $companyId})
                RETURN v";

			var vehicles = new List<Vehicle>();

			await using var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { companyId });
				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["v"].As<INode>();
					vehicles.Add(MapNodeToVehicle(node));
				}
			}
			finally
			{
				await session.CloseAsync();
			}
			return vehicles;
		}

		public async Task<Vehicle?> GetByIdAsync(int id)
		{
			var query = @"
                MATCH (v:Vehicle {Id: $id})
                RETURN v
                LIMIT 1";

			await using var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });
				if (await cursor.FetchAsync())
				{
					var node = cursor.Current["v"].As<INode>();
					return MapNodeToVehicle(node);
				}
				return null;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async void Update(Vehicle vehicle)
		{
			var query = @"
                MATCH (v:Vehicle {Id: $id})
                SET v.CompanyId = $companyId,
                    v.Brand = $brand,
                    v.Model = $model,
                    v.EngineCapacityCc = $engineCapacityCc,
                    v.VehicleTypeId = $vehicleTypeId,
                    v.RegistrationNumber = $registrationNumber,
                    v.CapacityKg = $capacityKg,
                    v.ManufactureYear = $manufactureYear,
                    v.VehicleStatus = $vehicleStatus,
                    v.LastInspectionDate = $lastInspectionDate,
                    v.InsuranceExpiry = $insuranceExpiry,
                    v.CurrentLocationId = $currentLocationId,
                    v.CreatedAt = $createdAt,
                    v.Categories = $categories";

			var parameters = new Dictionary<string, object?>
			{
				{ "id", vehicle.Id },
				{ "companyId", vehicle.CompanyId },
				{ "brand", vehicle.Brand },
				{ "model", vehicle.Model },
				{ "engineCapacityCc", vehicle.EngineCapacityCc },
				{ "vehicleTypeId", vehicle.VehicleTypeId },
				{ "registrationNumber", vehicle.RegistrationNumber },
				{ "capacityKg", vehicle.CapacityKg },
				{ "manufactureYear", vehicle.ManufactureYear },
				{ "vehicleStatus", (int)vehicle.VehicleStatus },
				{ "lastInspectionDate", vehicle.LastInspectionDate?.ToString("o") },
				{ "insuranceExpiry", vehicle.InsuranceExpiry?.ToString("o") },
				{ "currentLocationId", vehicle.CurrentLocationId },
				{ "createdAt", vehicle.CreatedAt.ToString("o") },
				{ "categories", vehicle.Categories ?? "" }
			};

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private Vehicle MapNodeToVehicle(INode node)
		{
			return new Vehicle
			{
				Id = (int)(long)node.Properties["Id"],
				CompanyId = (int)(long)node.Properties["CompanyId"],
				Brand = node.Properties["Brand"].As<string>(),
				Model = node.Properties["Model"].As<string>(),
				EngineCapacityCc = (int)(long)node.Properties["EngineCapacityCc"],
				VehicleTypeId = (int)(long)node.Properties["VehicleTypeId"],
				RegistrationNumber = node.Properties["RegistrationNumber"].As<string>(),
				CapacityKg = (int)(long)node.Properties["CapacityKg"],
				ManufactureYear = (int)(long)node.Properties["ManufactureYear"],
				VehicleStatus = (VehicleStatus)(int)(long)node.Properties["VehicleStatus"],
				LastInspectionDate = node.Properties.ContainsKey("LastInspectionDate") && node.Properties["LastInspectionDate"] != null
					? DateTime.Parse(node.Properties["LastInspectionDate"].As<string>())
					: (DateTime?)null,
				InsuranceExpiry = node.Properties.ContainsKey("InsuranceExpiry") && node.Properties["InsuranceExpiry"] != null
					? DateTime.Parse(node.Properties["InsuranceExpiry"].As<string>())
					: (DateTime?)null,
				CurrentLocationId = node.Properties.ContainsKey("CurrentLocationId") && node.Properties["CurrentLocationId"] != null
					? (int?)(long)node.Properties["CurrentLocationId"]
					: null,
				CreatedAt = DateTime.Parse(node.Properties["CreatedAt"].As<string>()),
				Categories = node.Properties.ContainsKey("Categories") ? node.Properties["Categories"].As<string>() : null
			};
		}
	}
}
