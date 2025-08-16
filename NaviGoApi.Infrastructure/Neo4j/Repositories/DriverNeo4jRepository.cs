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

		public async Task<IEnumerable<Driver>> GetAvailableDriversAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (d:Driver)
                WHERE d.DriverStatus = $Status
                RETURN d",
				new { Status = (int)DriverStatus.Available });

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
			return new Driver
			{
				Id = node.Properties["Id"].As<int>(),
				CompanyId = node.Properties["CompanyId"].As<int>(),
				FirstName = node.Properties["FirstName"].As<string>(),
				LastName = node.Properties["LastName"].As<string>(),
				PhoneNumber = node.Properties["PhoneNumber"].As<string>(),
				LicenseNumber = node.Properties["LicenseNumber"].As<string>(),
				LicenseExpiry = node.Properties.ContainsKey("LicenseExpiry") ? node.Properties["LicenseExpiry"].As<ZonedDateTime>().ToDateTimeOffset().LocalDateTime : null,
				LicenseCategories = node.Properties["LicenseCategories"].As<string>(),
				HireDate = node.Properties["HireDate"].As<ZonedDateTime>().ToDateTimeOffset().LocalDateTime,
				DriverStatus = (DriverStatus)node.Properties["DriverStatus"].As<int>()
			};
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
