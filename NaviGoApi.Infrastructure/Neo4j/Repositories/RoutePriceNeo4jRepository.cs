using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RoutePriceNeo4jRepository
	{
		private readonly IDriver _driver;

		public RoutePriceNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(RoutePrice price)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (r:Route {Id: $routeId}), (vt:VehicleType {Id: $vehicleTypeId})
                CREATE (rp:RoutePrice {
                    Id: $id,
                    PricePerKm: $pricePerKm,
                    MinimumPrice: $minimumPrice
                })
                CREATE (rp)-[:BELONGS_TO_ROUTE]->(r)
                CREATE (rp)-[:FOR_VEHICLE_TYPE]->(vt)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = price.Id,
				["routeId"] = price.RouteId,
				["vehicleTypeId"] = price.VehicleTypeId,
				["pricePerKm"] = price.PricePerKm,
				["minimumPrice"] = price.MinimumPrice
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task UpdateAsync(RoutePrice price)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (rp:RoutePrice {Id: $id})
                SET rp.PricePerKm = $pricePerKm,
                    rp.MinimumPrice = $minimumPrice
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = price.Id,
				["pricePerKm"] = price.PricePerKm,
				["minimumPrice"] = price.MinimumPrice
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (rp:RoutePrice {Id: $id}) DETACH DELETE rp";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (rp:RoutePrice {Id: $id})
                OPTIONAL MATCH (rp)-[:BELONGS_TO_ROUTE]->(r:Route)
                OPTIONAL MATCH (rp)-[:FOR_VEHICLE_TYPE]->(vt:VehicleType)
                RETURN rp, r, vt
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				if (!await cursor.FetchAsync()) return null;

				var record = cursor.Current;

				var rpNode = record["rp"].As<INode>();
				var rNode = record["r"]?.As<INode>();
				var vtNode = record["vt"]?.As<INode>();

				var routePrice = new RoutePrice
				{
					Id = (int)(long)rpNode.Properties["Id"],
					PricePerKm = Convert.ToDecimal((double)rpNode.Properties["PricePerKm"]),
					MinimumPrice = Convert.ToDecimal((double)rpNode.Properties["MinimumPrice"]),
					RouteId = rNode != null ? (int)(long)rNode.Properties["Id"] : 0,
					VehicleTypeId = vtNode != null ? (int)(long)vtNode.Properties["Id"] : 0,
				};

				if (rNode != null)
					routePrice.Route = new Route { Id = (int)(long)rNode.Properties["Id"] };

				if (vtNode != null)
					routePrice.VehicleType = new VehicleType { Id = (int)(long)vtNode.Properties["Id"] };

				return routePrice;
			});
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (rp:RoutePrice)
                OPTIONAL MATCH (rp)-[:BELONGS_TO_ROUTE]->(r:Route)
                OPTIONAL MATCH (rp)-[:FOR_VEHICLE_TYPE]->(vt:VehicleType)
                RETURN rp, r, vt
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<RoutePrice>();

				foreach (var record in records)
				{
					var rpNode = record["rp"].As<INode>();
					var rNode = record["r"]?.As<INode>();
					var vtNode = record["vt"]?.As<INode>();

					var routePrice = new RoutePrice
					{
						Id = (int)(long)rpNode.Properties["Id"],
						PricePerKm = Convert.ToDecimal((double)rpNode.Properties["PricePerKm"]),
						MinimumPrice = Convert.ToDecimal((double)rpNode.Properties["MinimumPrice"]),
						RouteId = rNode != null ? (int)(long)rNode.Properties["Id"] : 0,
						VehicleTypeId = vtNode != null ? (int)(long)vtNode.Properties["Id"] : 0,
					};

					if (rNode != null)
						routePrice.Route = new Route { Id = (int)(long)rNode.Properties["Id"] };

					if (vtNode != null)
						routePrice.VehicleType = new VehicleType { Id = (int)(long)vtNode.Properties["Id"] };

					list.Add(routePrice);
				}

				return list;
			});
		}
	}
}
