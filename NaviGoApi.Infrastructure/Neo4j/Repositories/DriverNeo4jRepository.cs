using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq;
using NaviGoApi.Common.DTOs;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class DriverNeo4jRepository : IDriverRepository
	{
		private readonly IDriver _driver;

		public DriverNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Driver driverEntity)
		{
			var id = await GetNextIdAsync("Driver");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (d:Driver {
                    Id: $Id,
                    CompanyId: $CompanyId,
                    FirstName: $FirstName,
                    LastName: $LastName,
                    PhoneNumber: $PhoneNumber,
                    LicenseNumber: $LicenseNumber,
                    LicenseExpiry: $LicenseExpiry,
                    LicenseCategories: $LicenseCategories,
                    HireDate: $HireDate,
                    DriverStatus: $DriverStatus
                })",
					new
					{
						Id = id,
						driverEntity.CompanyId,
						driverEntity.FirstName,
						driverEntity.LastName,
						driverEntity.PhoneNumber,
						driverEntity.LicenseNumber,
						driverEntity.LicenseExpiry,
						driverEntity.LicenseCategories,
						driverEntity.HireDate,
						DriverStatus = (int)driverEntity.DriverStatus
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Driver driverEntity)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (d:Driver {Id: $Id})
                DETACH DELETE d",
				new { driverEntity.Id });
		}

		public async Task<IEnumerable<Driver>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (d:Driver)
                RETURN d");

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["d"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<Driver>> GetAvailableDriversAsync(int companyId)
		{
			await using var session = _driver.AsyncSession();

			var cursor = await session.RunAsync(@"
        MATCH (d:Driver)
        WHERE d.DriverStatus = $Status AND d.CompanyId = $CompanyId
        RETURN d",
				new { Status = (int)DriverStatus.Available, CompanyId = companyId });

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["d"].As<INode>())).ToList();
		}


		public async Task<IEnumerable<Driver>> GetByCompanyIdAsync(int companyId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (d:Driver {CompanyId: $CompanyId})
                RETURN d",
				new { CompanyId = companyId });

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["d"].As<INode>())).ToList();
		}

		public async Task<Driver?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (d:Driver {Id: $Id})
                RETURN d",
				new { Id = id });

			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;

			return MapNodeToEntity(records[0]["d"].As<INode>());
		}

		public async Task UpdateAsync(Driver driverEntity)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (d:Driver {Id: $Id})
                SET d.CompanyId = $CompanyId,
                    d.FirstName = $FirstName,
                    d.LastName = $LastName,
                    d.PhoneNumber = $PhoneNumber,
                    d.LicenseNumber = $LicenseNumber,
                    d.LicenseExpiry = $LicenseExpiry,
                    d.LicenseCategories = $LicenseCategories,
                    d.HireDate = $HireDate,
                    d.DriverStatus = $DriverStatus",
				new
				{
					driverEntity.Id,
					driverEntity.CompanyId,
					driverEntity.FirstName,
					driverEntity.LastName,
					driverEntity.PhoneNumber,
					driverEntity.LicenseNumber,
					driverEntity.LicenseExpiry,
					driverEntity.LicenseCategories,
					driverEntity.HireDate,
					DriverStatus = (int)driverEntity.DriverStatus
				});
		}

		private Driver MapNodeToEntity(INode node)
		{
			if (node == null) return null!;

			// HireDate
			DateTime? hireDate = null;
			if (node.Properties.ContainsKey("HireDate") && node["HireDate"] != null)
			{
				var localDateTime = node["HireDate"].As<LocalDateTime>();
				hireDate = new DateTime(
					localDateTime.Year,
					localDateTime.Month,
					localDateTime.Day,
					localDateTime.Hour,
					localDateTime.Minute,
					localDateTime.Second
				);
			}

			// LicenseExpiry
			DateTime? licenseExpiry = null;
			if (node.Properties.ContainsKey("LicenseExpiry") && node["LicenseExpiry"] != null)
			{
				var localDateTime = node["LicenseExpiry"].As<LocalDateTime>();
				licenseExpiry = new DateTime(
					localDateTime.Year,
					localDateTime.Month,
					localDateTime.Day,
					localDateTime.Hour,
					localDateTime.Minute,
					localDateTime.Second
				);
			}

			// Mapiranje Driver entiteta
			var driver = new Driver
			{
				Id = node["Id"].As<int>(),
				CompanyId = node["CompanyId"].As<int>(),
				FirstName = node["FirstName"].As<string>(),
				LastName = node["LastName"].As<string>(),
				PhoneNumber = node["PhoneNumber"].As<string>(),
				LicenseNumber = node["LicenseNumber"].As<string>(),
				LicenseCategories = node["LicenseCategories"].As<string>(),
				HireDate = hireDate ?? DateTime.MinValue,
				LicenseExpiry = licenseExpiry,
				DriverStatus = Enum.TryParse<DriverStatus>(node["DriverStatus"].As<string>(), out var status)
							   ? status
							   : DriverStatus.Available
			};

			return driver;
		}

		public async Task<IEnumerable<Driver>> GetAllAsync(DriverSearchDto driverSearch)
		{
			var filters = new List<string>();
			var parameters = new Dictionary<string, object>();

			if (!string.IsNullOrWhiteSpace(driverSearch.FirstName))
			{
				filters.Add("toLower(d.FirstName) CONTAINS toLower($FirstName)");
				parameters["FirstName"] = driverSearch.FirstName;
			}

			if (!string.IsNullOrWhiteSpace(driverSearch.LastName))
			{
				filters.Add("toLower(d.LastName) CONTAINS toLower($LastName)");
				parameters["LastName"] = driverSearch.LastName;
			}

			string whereClause = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : "";

			// Sortiranje
			string sortField = driverSearch.SortBy?.ToLower() switch
			{
				"firstname" => "d.FirstName",
				"lastname" => "d.LastName",
				_ => "d.Id"
			};

			string sortDirection = driverSearch.SortDirection.ToLower() == "desc" ? "DESC" : "ASC";

			int skip = (driverSearch.Page - 1) * driverSearch.PageSize;
			int limit = driverSearch.PageSize;

			string query = $@"
        MATCH (d:Driver)
        {whereClause}
        RETURN d
        ORDER BY {sortField} {sortDirection}
        SKIP $skip
        LIMIT $limit";

			parameters["skip"] = skip;
			parameters["limit"] = limit;

			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(query, parameters);

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["d"].As<INode>())).ToList();
		}

	}
}
