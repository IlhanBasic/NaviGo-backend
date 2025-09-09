
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RouteNeo4jRepository : IRouteRepository
	{
		private readonly IDriver _driver;

		public RouteNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Route route)
		{
			var id = await GetNextIdAsync("Route");

			var query = @"
                CREATE (r:Route {
                    Id: $Id,
                    CompanyId: $CompanyId,
                    StartLocationId: $StartLocationId,
                    EndLocationId: $EndLocationId,
                    DistanceKm: $DistanceKm,
                    EstimatedDurationHours: $EstimatedDurationHours,
                    IsActive: $IsActive,
                    CreatedAt: $CreatedAt,
                    AvailableFrom: $AvailableFrom,
                    AvailableTo: $AvailableTo,
                    GeometryEncoded: $GeometryEncoded
                })
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					route.CompanyId,
					route.StartLocationId,
					route.EndLocationId,
					route.DistanceKm,
					route.EstimatedDurationHours,
					route.IsActive,
					route.CreatedAt,
					route.AvailableFrom,
					route.AvailableTo,
					route.GeometryEncoded,
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public Task UpdateAsync(Route route)
		{
			var query = @"
                MATCH (r:Route { Id: $Id })
                SET r.CompanyId = $CompanyId,
                    r.StartLocationId = $StartLocationId,
                    r.EndLocationId = $EndLocationId,
                    r.DistanceKm = $DistanceKm,
                    r.EstimatedDurationHours = $EstimatedDurationHours,
                    r.IsActive = $IsActive,
                    r.CreatedAt = $CreatedAt,
                    r.AvailableFrom = $AvailableFrom,
                    r.AvailableTo = $AvailableTo,
                    r.GeometryEncoded = $GeometryEncoded
            ";

			var session = _driver.AsyncSession();
			return session.RunAsync(query, new
			{
				route.Id,
				route.CompanyId,
				route.StartLocationId,
				route.EndLocationId,
				route.DistanceKm,
				route.EstimatedDurationHours,
				route.IsActive,
				route.CreatedAt,
				route.AvailableFrom,
				route.AvailableTo,
				route.GeometryEncoded
			});
		}

		public async Task DeleteAsync(Route route)
		{
			var query = @"MATCH (r:Route { Id: $Id }) DETACH DELETE r";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { route.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			var query = @"
                MATCH (r:Route { Id: $Id })
                RETURN r
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				return MapRouteNode(record["r"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			var query = @"MATCH (r:Route) RETURN r";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var records = await result.ToListAsync();
				return records.Select(r => MapRouteNode(r["r"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			var now = DateTime.UtcNow;
			var query = @"
                MATCH (r:Route)
                WHERE r.IsActive = true AND r.AvailableFrom <= $Now AND r.AvailableTo >= $Now
                RETURN r
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Now = now });
				var records = await result.ToListAsync();
				return records.Select(r => MapRouteNode(r["r"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			var query = @"MATCH (r:Route { CompanyId: $CompanyId }) RETURN r";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { CompanyId = companyId });
				var records = await result.ToListAsync();
				return records.Select(r => MapRouteNode(r["r"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private async Task<int> GetNextIdAsync(string entityName)
		{
			var query = @"
                MERGE (c:Counter { name: $entityName })
                ON CREATE SET c.lastId = 1
                ON MATCH SET c.lastId = c.lastId + 1
                RETURN c.lastId AS lastId
            ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { entityName });
				var record = (await result.ToListAsync()).First();
				return record["lastId"].As<int>();
			}
			finally
			{
				await session.CloseAsync();
			}
		}
		private DateTime ConvertNeo4jDate(object value)
		{
			if (value is ZonedDateTime zdt)
				return zdt.ToDateTimeOffset().LocalDateTime;
			if (value is LocalDateTime ldt)
				return ldt.ToDateTime();

			throw new InvalidCastException($"Unsupported date type: {value?.GetType()}");
		}

		private Route MapRouteNode(INode node)
		{
			return new Route
			{
				Id = node["Id"].As<int>(),
				CompanyId = node["CompanyId"].As<int>(),
				StartLocationId = node["StartLocationId"].As<int>(),
				EndLocationId = node["EndLocationId"].As<int>(),
				DistanceKm = node["DistanceKm"].As<double>(),
				EstimatedDurationHours = node["EstimatedDurationHours"].As<double>(),
				IsActive = node["IsActive"].As<bool>(),
				CreatedAt = ConvertNeo4jDate(node["CreatedAt"]),
				AvailableFrom = ConvertNeo4jDate(node["AvailableFrom"]),
				AvailableTo = ConvertNeo4jDate(node["AvailableTo"]),
				GeometryEncoded = node["GeometryEncoded"].As<string?>(),
				//GeometryJson = node["GeometryJson"].As<string?>()
			};
		}

		public async Task<bool> DuplicateRoute(int companyId, int startLocationId, int endLocationId)
		{
			var query = @"
        MATCH (r:Route)
        WHERE r.CompanyId = $CompanyId 
          AND r.StartLocationId = $StartLocationId 
          AND r.EndLocationId = $EndLocationId
        RETURN count(r) > 0 AS exists
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new
				{
					CompanyId = companyId,
					StartLocationId = startLocationId,
					EndLocationId = endLocationId
				});

				var record = await result.SingleAsync();
				return record["exists"].As<bool>();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<bool> DuplicateRouteUpdate(int companyId, int startLocationId, int endLocationId, int routeId)
		{
			var query = @"
        MATCH (r:Route)
        WHERE r.CompanyId = $CompanyId 
          AND r.StartLocationId = $StartLocationId 
          AND r.EndLocationId = $EndLocationId
          AND r.Id <> $RouteId
        RETURN count(r) > 0 AS exists
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new
				{
					CompanyId = companyId,
					StartLocationId = startLocationId,
					EndLocationId = endLocationId,
					RouteId = routeId
				});

				var record = await result.SingleAsync();
				return record["exists"].As<bool>();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Route>> GetAllAsync(RouteSearchDto routeSearch)
		{
			string sortBy = string.IsNullOrEmpty(routeSearch.SortBy) ? "Id" : routeSearch.SortBy;
			string sortDirection = routeSearch.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";
			int skip = (routeSearch.Page - 1) * routeSearch.PageSize;
			int limit = routeSearch.PageSize;

			var query = $@"
        MATCH (r:Route)
        RETURN r
        ORDER BY r.{sortBy} {sortDirection}
        SKIP $Skip
        LIMIT $Limit
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Skip = skip, Limit = limit });
				var records = await result.ToListAsync();
				return records.Select(r => MapRouteNode(r["r"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

	}
}
