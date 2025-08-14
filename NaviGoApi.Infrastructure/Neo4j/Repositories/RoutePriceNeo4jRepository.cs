//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Neo4j.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;
//using System.Linq.Expressions;

//namespace NaviGoApi.Infrastructure.Neo4j.Repositories
//{
//	public class RoutePriceNeo4jRepository : IRoutePriceRepository
//	{
//		private readonly IDriver _driver;

//		public RoutePriceNeo4jRepository(IDriver driver)
//		{
//			_driver = driver;
//		}

//private async Task<int> GetNextIdAsync(string entityName)
//{
//	var query = @"
//            MERGE (c:Counter { name: $entityName })
//            ON CREATE SET c.lastId = 1
//            ON MATCH SET c.lastId = c.lastId + 1
//            RETURN c.lastId as lastId
//        ";

//	var session = _driver.AsyncSession();
//	try
//	{
//		var result = await session.RunAsync(query, new { entityName });
//		var record = await result.SingleAsync();
//		return record["lastId"].As<int>();
//	}
//	finally
//	{
//		await session.CloseAsync();
//	}
//}

//		public async Task AddAsync(RoutePrice price)
//		{
//			var id = await GetNextIdAsync("RoutePrice");

//			await using var session = _driver.AsyncSession();
//			try
//			{
//				await session.RunAsync(@"
//                CREATE (rp:RoutePrice {
//                    Id: $Id,
//                    RouteId: $RouteId,
//                    VehicleTypeId: $VehicleTypeId,
//                    PricePerKm: $PricePerKm,
//                    MinimumPrice: $MinimumPrice
//                })",
//					new
//					{
//						Id = id,
//						price.RouteId,
//						price.VehicleTypeId,
//						price.PricePerKm,
//						price.MinimumPrice
//					});
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task DeleteAsync(int id)
//		{
//			await using var session = _driver.AsyncSession();
//			await session.RunAsync(@"
//				MATCH (rp:RoutePrice {Id: $Id})
//				DETACH DELETE rp",
//				new { Id = id });
//		}

//		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
//		{
//			await using var session = _driver.AsyncSession();
//			var cursor = await session.RunAsync("MATCH (rp:RoutePrice) RETURN rp");
//			var records = await cursor.ToListAsync();
//			return records.Select(r => MapNodeToEntity(r["rp"].As<INode>())).ToList();
//		}

//		public async Task<RoutePrice?> GetByIdAsync(int id)
//		{
//			await using var session = _driver.AsyncSession();
//			var cursor = await session.RunAsync("MATCH (rp:RoutePrice {Id: $Id}) RETURN rp", new { Id = id });
//			var records = await cursor.ToListAsync();
//			if (records.Count == 0) return null;
//			return MapNodeToEntity(records[0]["rp"].As<INode>());
//		}

//		public async Task UpdateAsync(RoutePrice price)
//		{
//			await using var session = _driver.AsyncSession();
//			await session.RunAsync(@"
//				MATCH (rp:RoutePrice {Id: $Id})
//				SET rp.RouteId = $RouteId,
//					rp.VehicleTypeId = $VehicleTypeId,
//					rp.PricePerKm = $PricePerKm,
//					rp.MinimumPrice = $MinimumPrice",
//				new
//				{
//					price.Id,
//					price.RouteId,
//					price.VehicleTypeId,
//					price.PricePerKm,
//					price.MinimumPrice
//				});
//		}

//		private RoutePrice MapNodeToEntity(INode node)
//		{
//			return new RoutePrice
//			{
//				Id = node.Properties["Id"].As<int>(),
//				RouteId = node.Properties["RouteId"].As<int>(),
//				VehicleTypeId = node.Properties["VehicleTypeId"].As<int>(),
//				PricePerKm = Convert.ToDecimal(node.Properties["PricePerKm"]),
//				MinimumPrice = Convert.ToDecimal(node.Properties["MinimumPrice"])
//			};
//		}

//		public Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate)
//		{
//			throw new NotImplementedException();
//		}
//	}
//}
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RoutePriceNeo4jRepository : IRoutePriceRepository
	{
		private readonly IDriver _driver;

		public RoutePriceNeo4jRepository(IDriver driver)
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
		public async Task AddAsync(RoutePrice price)
		{
			var id = await GetNextIdAsync("RoutePrice");
			var query = @"
                CREATE (rp:RoutePrice {
                    Id: $id,
                    RouteId: $routeId,
                    VehicleTypeId: $vehicleTypeId,
                    PricePerKm: $pricePerKm,
                    MinimumPrice: $minimumPrice
                })";
			var session = _driver.AsyncSession();
			await session.RunAsync(query, new
			{
				id = id,
				routeId = price.RouteId,
				vehicleTypeId = price.VehicleTypeId,
				pricePerKm = price.PricePerKm,
				minimumPrice = price.MinimumPrice
			});
			await session.CloseAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"MATCH (rp:RoutePrice {Id: $id}) DETACH DELETE rp";
			var session = _driver.AsyncSession();
			await session.RunAsync(query, new { id });
			await session.CloseAsync();
		}

		public async Task<RoutePrice?> DuplicateRoutePrice(int routeId, int vehicleTypeId)
		{
			var query = @"
        MATCH (rp:RoutePrice)
        WHERE rp.RouteId = $routeId
          AND rp.VehicleTypeId = $vehicleTypeId
        RETURN rp
        LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new
				{
					routeId,
					vehicleTypeId
				});

				if (!await result.FetchAsync())
					return null;

				var node = result.Current["rp"].As<INode>();
				return new RoutePrice
				{
					Id = node.Properties["Id"].As<int>(),
					RouteId = node.Properties["RouteId"].As<int>(),
					VehicleTypeId = node.Properties["VehicleTypeId"].As<int>(),
					PricePerKm = node.Properties["PricePerKm"].As<decimal>(),
					MinimumPrice = node.Properties["MinimumPrice"].As<decimal>()
				};
			}
			finally
			{
				await session.CloseAsync();
			}
		}


		public async Task<bool> ExistsAsync(int routeId, int vehicleTypeId)
		{
			var query = @"MATCH (rp:RoutePrice {RouteId: $routeId, VehicleTypeId: $vehicleTypeId}) RETURN rp LIMIT 1";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { routeId, vehicleTypeId });
			var exists = await result.FetchAsync();
			await session.CloseAsync();
			return exists;
		}

		public Task<bool> ExistsAsync(Expression<Func<RoutePrice, bool>> predicate)
		{
			throw new ValidationException("ExistsAsync sa Expression isn't possible in Neo4j.");
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			var query = @"MATCH (rp:RoutePrice) RETURN rp";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query);

			var list = new List<RoutePrice>();
			while (await result.FetchAsync())
			{
				var node = result.Current["rp"].As<INode>();
				list.Add(new RoutePrice
				{
					Id = node.Properties["Id"].As<int>(),
					RouteId = node.Properties["RouteId"].As<int>(),
					VehicleTypeId = node.Properties["VehicleTypeId"].As<int>(),
					PricePerKm = node.Properties["PricePerKm"].As<decimal>(),
					MinimumPrice = node.Properties["MinimumPrice"].As<decimal>()
				});
			}

			await session.CloseAsync();
			return list;
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			var query = @"MATCH (rp:RoutePrice {Id: $id}) RETURN rp LIMIT 1";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { id });
			if (!await result.FetchAsync())
			{
				await session.CloseAsync();
				return null;
			}

			var node = result.Current["rp"].As<INode>();
			await session.CloseAsync();

			return new RoutePrice
			{
				Id = node.Properties["Id"].As<int>(),
				RouteId = node.Properties["RouteId"].As<int>(),
				VehicleTypeId = node.Properties["VehicleTypeId"].As<int>(),
				PricePerKm = node.Properties["PricePerKm"].As<decimal>(),
				MinimumPrice = node.Properties["MinimumPrice"].As<decimal>()
			};
		}

		public Task UpdateAsync(RoutePrice price)
		{
			var query = @"
                MATCH (rp:RoutePrice {Id: $id})
                SET rp.RouteId = $routeId,
                    rp.VehicleTypeId = $vehicleTypeId,
                    rp.PricePerKm = $pricePerKm,
                    rp.MinimumPrice = $minimumPrice";
			var session = _driver.AsyncSession();
			var task = session.RunAsync(query, new
			{
				id = price.Id,
				routeId = price.RouteId,
				vehicleTypeId = price.VehicleTypeId,
				pricePerKm = price.PricePerKm,
				minimumPrice = price.MinimumPrice
			});
			return task.ContinueWith(async t => await session.CloseAsync());
		}
	}
}
