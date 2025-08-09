using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class DriverNeo4jRepository
	{
		private readonly IDriver _driver;

		public DriverNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Driver driver)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (d:Driver {
                    Id: $id,
                    CompanyId: $companyId,
                    FirstName: $firstName,
                    LastName: $lastName,
                    PhoneNumber: $phoneNumber,
                    LicenseNumber: $licenseNumber,
                    LicenseExpiry: $licenseExpiry,
                    LicenseCategories: $licenseCategories,
                    HireDate: $hireDate,
                    DriverStatus: $driverStatus
                })
                WITH d
                MATCH (c:Company {Id: $companyId})
                CREATE (d)-[:WORKS_FOR]->(c)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = driver.Id,
				["companyId"] = driver.CompanyId,
				["firstName"] = driver.FirstName,
				["lastName"] = driver.LastName,
				["phoneNumber"] = driver.PhoneNumber,
				["licenseNumber"] = driver.LicenseNumber,
				["licenseExpiry"] = driver.LicenseExpiry?.ToString("o"),
				["licenseCategories"] = driver.LicenseCategories,
				["hireDate"] = driver.HireDate.ToString("o"),
				["driverStatus"] = (int)driver.DriverStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (d:Driver {Id: $id}) DETACH DELETE d";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<Driver>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (d:Driver)-[:WORKS_FOR]->(c:Company)
                RETURN d, c
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var drivers = new List<Driver>();

				foreach (var record in records)
				{
					var nodeDriver = record["d"].As<INode>();
					var nodeCompany = record["c"].As<INode>();

					var driver = MapNodeToDriver(nodeDriver);
					driver.Company = new Company { Id = (int)(long)nodeCompany.Properties["Id"] };

					drivers.Add(driver);
				}

				return drivers;
			});
		}

		public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (d:Driver)-[:WORKS_FOR]->(c:Company {Id: $companyId})
                RETURN d, c
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { companyId });
				var records = await cursor.ToListAsync();

				var drivers = new List<Driver>();

				foreach (var record in records)
				{
					var nodeDriver = record["d"].As<INode>();
					var nodeCompany = record["c"].As<INode>();

					var driver = MapNodeToDriver(nodeDriver);
					driver.Company = new Company { Id = (int)(long)nodeCompany.Properties["Id"] };

					drivers.Add(driver);
				}

				return drivers;
			});
		}

		public async Task<Driver?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (d:Driver {Id: $id})-[:WORKS_FOR]->(c:Company)
                RETURN d, c
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();

				if (!hasRecord) return null;

				var record = cursor.Current;

				var nodeDriver = record["d"].As<INode>();
				var nodeCompany = record["c"].As<INode>();

				var driver = MapNodeToDriver(nodeDriver);
				driver.Company = new Company { Id = (int)(long)nodeCompany.Properties["Id"] };

				return driver;
			});
		}

		public async Task UpdateAsync(Driver driver)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (d:Driver {Id: $id})
                SET d.CompanyId = $companyId,
                    d.FirstName = $firstName,
                    d.LastName = $lastName,
                    d.PhoneNumber = $phoneNumber,
                    d.LicenseNumber = $licenseNumber,
                    d.LicenseExpiry = $licenseExpiry,
                    d.LicenseCategories = $licenseCategories,
                    d.HireDate = $hireDate,
                    d.DriverStatus = $driverStatus
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = driver.Id,
				["companyId"] = driver.CompanyId,
				["firstName"] = driver.FirstName,
				["lastName"] = driver.LastName,
				["phoneNumber"] = driver.PhoneNumber,
				["licenseNumber"] = driver.LicenseNumber,
				["licenseExpiry"] = driver.LicenseExpiry?.ToString("o"),
				["licenseCategories"] = driver.LicenseCategories,
				["hireDate"] = driver.HireDate.ToString("o"),
				["driverStatus"] = (int)driver.DriverStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private Driver MapNodeToDriver(INode node)
		{
			return new Driver
			{
				Id = (int)(long)node.Properties["Id"],
				CompanyId = (int)(long)node.Properties["CompanyId"],
				FirstName = (string)node.Properties["FirstName"],
				LastName = (string)node.Properties["LastName"],
				PhoneNumber = (string)node.Properties["PhoneNumber"],
				LicenseNumber = (string)node.Properties["LicenseNumber"],
				LicenseExpiry = node.Properties.ContainsKey("LicenseExpiry") && node.Properties["LicenseExpiry"] != null
					? DateTime.Parse((string)node.Properties["LicenseExpiry"])
					: (DateTime?)null,
				LicenseCategories = (string)node.Properties["LicenseCategories"],
				HireDate = DateTime.Parse((string)node.Properties["HireDate"]),
				DriverStatus = (DriverStatus)(int)(long)node.Properties["DriverStatus"]
			};
		}
	}
}
