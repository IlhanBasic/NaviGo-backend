using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq.Expressions;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RouteNeo4jRepository : IRouteRepository
	{
		private readonly IDriver _driver;

		public RouteNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Route route)
		{
			var id = await GetNextIdAsync("Route");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (r:Route {
                    Id: $Id,
                    CompanyId: $CompanyId,
                    StartLocationId: $StartLocationId,
                    EndLocationId: $EndLocationId,
                    DistanceKm: $DistanceKm,
                    EstimatedDurationHours: $EstimatedDurationHours,
                    BasePrice: $BasePrice,
                    IsActive: $IsActive,
                    CreatedAt: datetime($CreatedAt),
                    AvailableFrom: datetime($AvailableFrom),
                    AvailableTo: datetime($AvailableTo),
                    GeometryEncoded: $GeometryEncoded,
                    GeometryJson: $GeometryJson
                })",
					new
					{
						Id = id,
						route.CompanyId,
						route.StartLocationId,
						route.EndLocationId,
						route.DistanceKm,
						route.EstimatedDurationHours,
						route.BasePrice,
						route.IsActive,
						CreatedAt = route.CreatedAt.ToString("o"),
						AvailableFrom = route.AvailableFrom.ToString("o"),
						AvailableTo = route.AvailableTo.ToString("o"),
						route.GeometryEncoded,
						route.GeometryJson
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Route route)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (r:Route {Id: $Id})
				DETACH DELETE r",
				new { route.Id });
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (r:Route) RETURN r");
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["r"].As<INode>())).ToList();
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (r:Route {Id: $Id}) RETURN r", new { Id = id });
			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;
			return MapNodeToEntity(records[0]["r"].As<INode>());
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			await using var session = _driver.AsyncSession();
			var now = DateTime.UtcNow.ToString("o");
			var cursor = await session.RunAsync(@"
				MATCH (r:Route)
				WHERE r.IsActive = true AND r.AvailableFrom <= datetime($now) AND r.AvailableTo >= datetime($now)
				RETURN r",
				new { now });
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["r"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (r:Route {CompanyId: $CompanyId}) RETURN r", new { CompanyId = companyId });
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["r"].As<INode>())).ToList();
		}

		public async Task UpdateAsync(Route route)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (r:Route {Id: $Id})
				SET r.CompanyId = $CompanyId,
					r.StartLocationId = $StartLocationId,
					r.EndLocationId = $EndLocationId,
					r.DistanceKm = $DistanceKm,
					r.EstimatedDurationHours = $EstimatedDurationHours,
					r.BasePrice = $BasePrice,
					r.IsActive = $IsActive,
					r.CreatedAt = datetime($CreatedAt),
					r.AvailableFrom = datetime($AvailableFrom),
					r.AvailableTo = datetime($AvailableTo),
					r.GeometryEncoded = $GeometryEncoded,
					r.GeometryJson = $GeometryJson",
				new
				{
					route.Id,
					route.CompanyId,
					route.StartLocationId,
					route.EndLocationId,
					route.DistanceKm,
					route.EstimatedDurationHours,
					route.BasePrice,
					route.IsActive,
					CreatedAt = route.CreatedAt.ToString("o"),
					AvailableFrom = route.AvailableFrom.ToString("o"),
					AvailableTo = route.AvailableTo.ToString("o"),
					route.GeometryEncoded,
					route.GeometryJson
				});
		}

		private Route MapNodeToEntity(INode node)
		{
			return new Route
			{
				Id = node.Properties["Id"].As<int>(),
				CompanyId = node.Properties["CompanyId"].As<int>(),
				StartLocationId = node.Properties["StartLocationId"].As<int>(),
				EndLocationId = node.Properties["EndLocationId"].As<int>(),
				DistanceKm = node.Properties["DistanceKm"].As<double>(),
				EstimatedDurationHours = node.Properties["EstimatedDurationHours"].As<double>(),
				BasePrice = Convert.ToDecimal(node.Properties["BasePrice"]),
				IsActive = node.Properties["IsActive"].As<bool>(),
				CreatedAt = DateTime.Parse(node.Properties["CreatedAt"].As<string>()),
				AvailableFrom = DateTime.Parse(node.Properties["AvailableFrom"].As<string>()),
				AvailableTo = DateTime.Parse(node.Properties["AvailableTo"].As<string>()),
				GeometryEncoded = node.Properties.ContainsKey("GeometryEncoded") ? node.Properties["GeometryEncoded"].As<string>() : null,
				GeometryJson = node.Properties.ContainsKey("GeometryJson") ? node.Properties["GeometryJson"].As<string>() : null
			};
		}

		public Task<bool> ExistsAsync(Expression<Func<Route, bool>> predicate)
		{
			throw new NotImplementedException();
		}
	}
}
