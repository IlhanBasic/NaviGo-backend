using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RouteNeo4jRepository
	{
		private readonly IDriver _driver;

		public RouteNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Route route)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (r:Route {
                    Id: $id,
                    CompanyId: $companyId,
                    StartLocationId: $startLocationId,
                    EndLocationId: $endLocationId,
                    DistanceKm: $distanceKm,
                    EstimatedDurationHours: $estimatedDurationHours,
                    BasePrice: $basePrice,
                    IsActive: $isActive,
                    CreatedAt: $createdAt,
                    AvailableFrom: $availableFrom,
                    AvailableTo: $availableTo,
                    GeometryEncoded: $geometryEncoded,
                    GeometryJson: $geometryJson
                })
                WITH r
                MATCH (c:Company {Id: $companyId}), (start:Location {Id: $startLocationId}), (end:Location {Id: $endLocationId})
                CREATE (r)-[:BELONGS_TO]->(c)
                CREATE (r)-[:STARTS_AT]->(start)
                CREATE (r)-[:ENDS_AT]->(end)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = route.Id,
				["companyId"] = route.CompanyId,
				["startLocationId"] = route.StartLocationId,
				["endLocationId"] = route.EndLocationId,
				["distanceKm"] = route.DistanceKm,
				["estimatedDurationHours"] = route.EstimatedDurationHours,
				["basePrice"] = route.BasePrice,
				["isActive"] = route.IsActive,
				["createdAt"] = route.CreatedAt.ToString("o"),
				["availableFrom"] = route.AvailableFrom.ToString("o"),
				["availableTo"] = route.AvailableTo.ToString("o"),
				["geometryEncoded"] = route.GeometryEncoded ?? "",
				["geometryJson"] = route.GeometryJson ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task UpdateAsync(Route route)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (r:Route {Id: $id})
                SET r.CompanyId = $companyId,
                    r.StartLocationId = $startLocationId,
                    r.EndLocationId = $endLocationId,
                    r.DistanceKm = $distanceKm,
                    r.EstimatedDurationHours = $estimatedDurationHours,
                    r.BasePrice = $basePrice,
                    r.IsActive = $isActive,
                    r.CreatedAt = $createdAt,
                    r.AvailableFrom = $availableFrom,
                    r.AvailableTo = $availableTo,
                    r.GeometryEncoded = $geometryEncoded,
                    r.GeometryJson = $geometryJson
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = route.Id,
				["companyId"] = route.CompanyId,
				["startLocationId"] = route.StartLocationId,
				["endLocationId"] = route.EndLocationId,
				["distanceKm"] = route.DistanceKm,
				["estimatedDurationHours"] = route.EstimatedDurationHours,
				["basePrice"] = route.BasePrice,
				["isActive"] = route.IsActive,
				["createdAt"] = route.CreatedAt.ToString("o"),
				["availableFrom"] = route.AvailableFrom.ToString("o"),
				["availableTo"] = route.AvailableTo.ToString("o"),
				["geometryEncoded"] = route.GeometryEncoded ?? "",
				["geometryJson"] = route.GeometryJson ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (r:Route {Id: $id}) DETACH DELETE r";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<Route?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (r:Route {Id: $id})
                OPTIONAL MATCH (r)-[:BELONGS_TO]->(c:Company)
                OPTIONAL MATCH (r)-[:STARTS_AT]->(start:Location)
                OPTIONAL MATCH (r)-[:ENDS_AT]->(end:Location)
                RETURN r, c, start, end
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				if (!await cursor.FetchAsync()) return null;

				var record = cursor.Current;

				var rNode = record["r"].As<INode>();
				var cNode = record["c"]?.As<INode>();
				var startNode = record["start"]?.As<INode>();
				var endNode = record["end"]?.As<INode>();

				var route = MapNodeToRoute(rNode);

				if (cNode != null)
					route.Company = new Company { Id = (int)(long)cNode.Properties["Id"] };

				if (startNode != null)
					route.StartLocation = new Location { Id = (int)(long)startNode.Properties["Id"] };

				if (endNode != null)
					route.EndLocation = new Location { Id = (int)(long)endNode.Properties["Id"] };

				return route;
			});
		}

		public async Task<IEnumerable<Route>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (r:Route)
                OPTIONAL MATCH (r)-[:BELONGS_TO]->(c:Company)
                OPTIONAL MATCH (r)-[:STARTS_AT]->(start:Location)
                OPTIONAL MATCH (r)-[:ENDS_AT]->(end:Location)
                RETURN r, c, start, end
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<Route>();

				foreach (var record in records)
				{
					var rNode = record["r"].As<INode>();
					var cNode = record["c"]?.As<INode>();
					var startNode = record["start"]?.As<INode>();
					var endNode = record["end"]?.As<INode>();

					var route = MapNodeToRoute(rNode);

					if (cNode != null)
						route.Company = new Company { Id = (int)(long)cNode.Properties["Id"] };

					if (startNode != null)
						route.StartLocation = new Location { Id = (int)(long)startNode.Properties["Id"] };

					if (endNode != null)
						route.EndLocation = new Location { Id = (int)(long)endNode.Properties["Id"] };

					list.Add(route);
				}

				return list;
			});
		}

		public async Task<IEnumerable<Route>> GetActiveRoutesAsync()
		{
			await using var session = _driver.AsyncSession();

			var nowStr = DateTime.UtcNow.ToString("o");

			var query = @"
                MATCH (r:Route)
                WHERE r.IsActive = true AND r.AvailableFrom <= datetime($now) AND r.AvailableTo >= datetime($now)
                OPTIONAL MATCH (r)-[:BELONGS_TO]->(c:Company)
                OPTIONAL MATCH (r)-[:STARTS_AT]->(start:Location)
                OPTIONAL MATCH (r)-[:ENDS_AT]->(end:Location)
                RETURN r, c, start, end
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { now = nowStr });
				var records = await cursor.ToListAsync();

				var list = new List<Route>();

				foreach (var record in records)
				{
					var rNode = record["r"].As<INode>();
					var cNode = record["c"]?.As<INode>();
					var startNode = record["start"]?.As<INode>();
					var endNode = record["end"]?.As<INode>();

					var route = MapNodeToRoute(rNode);

					if (cNode != null)
						route.Company = new Company { Id = (int)(long)cNode.Properties["Id"] };

					if (startNode != null)
						route.StartLocation = new Location { Id = (int)(long)startNode.Properties["Id"] };

					if (endNode != null)
						route.EndLocation = new Location { Id = (int)(long)endNode.Properties["Id"] };

					list.Add(route);
				}

				return list;
			});
		}

		public async Task<IEnumerable<Route>> GetRoutesByCompanyIdAsync(int companyId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (r:Route)-[:BELONGS_TO]->(c:Company {Id: $companyId})
                OPTIONAL MATCH (r)-[:STARTS_AT]->(start:Location)
                OPTIONAL MATCH (r)-[:ENDS_AT]->(end:Location)
                RETURN r, c, start, end
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { companyId });
				var records = await cursor.ToListAsync();

				var list = new List<Route>();

				foreach (var record in records)
				{
					var rNode = record["r"].As<INode>();
					var cNode = record["c"].As<INode>();
					var startNode = record["start"]?.As<INode>();
					var endNode = record["end"]?.As<INode>();

					var route = MapNodeToRoute(rNode);

					route.Company = new Company { Id = (int)(long)cNode.Properties["Id"] };

					if (startNode != null)
						route.StartLocation = new Location { Id = (int)(long)startNode.Properties["Id"] };

					if (endNode != null)
						route.EndLocation = new Location { Id = (int)(long)endNode.Properties["Id"] };

					list.Add(route);
				}

				return list;
			});
		}

		private Route MapNodeToRoute(INode node)
		{
			return new Route
			{
				Id = (int)(long)node.Properties["Id"],
				CompanyId = (int)(long)node.Properties["CompanyId"],
				StartLocationId = (int)(long)node.Properties["StartLocationId"],
				EndLocationId = (int)(long)node.Properties["EndLocationId"],
				DistanceKm = (double)node.Properties["DistanceKm"],
				EstimatedDurationHours = (double)node.Properties["EstimatedDurationHours"],
				BasePrice = Convert.ToDecimal((double)node.Properties["BasePrice"]),
				IsActive = (bool)node.Properties["IsActive"],
				CreatedAt = DateTime.Parse((string)node.Properties["CreatedAt"]),
				AvailableFrom = DateTime.Parse((string)node.Properties["AvailableFrom"]),
				AvailableTo = DateTime.Parse((string)node.Properties["AvailableTo"]),
				GeometryEncoded = node.Properties.ContainsKey("GeometryEncoded") ? (string)node.Properties["GeometryEncoded"] : null,
				GeometryJson = node.Properties.ContainsKey("GeometryJson") ? (string)node.Properties["GeometryJson"] : null
			};
		}
	}
}
