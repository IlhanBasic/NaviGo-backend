using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class RoutePriceNeo4jRepository : IRoutePriceRepository
	{
		private readonly IDriver _driver;

		public RoutePriceNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(RoutePrice price)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				CREATE (rp:RoutePrice {
					Id: $Id,
					RouteId: $RouteId,
					VehicleTypeId: $VehicleTypeId,
					PricePerKm: $PricePerKm,
					MinimumPrice: $MinimumPrice
				})",
				new
				{
					price.Id,
					price.RouteId,
					price.VehicleTypeId,
					price.PricePerKm,
					price.MinimumPrice
				});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (rp:RoutePrice {Id: $Id})
				DETACH DELETE rp",
				new { Id = id });
		}

		public async Task<IEnumerable<RoutePrice>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (rp:RoutePrice) RETURN rp");
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["rp"].As<INode>())).ToList();
		}

		public async Task<RoutePrice?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (rp:RoutePrice {Id: $Id}) RETURN rp", new { Id = id });
			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;
			return MapNodeToEntity(records[0]["rp"].As<INode>());
		}

		public async Task UpdateAsync(RoutePrice price)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (rp:RoutePrice {Id: $Id})
				SET rp.RouteId = $RouteId,
					rp.VehicleTypeId = $VehicleTypeId,
					rp.PricePerKm = $PricePerKm,
					rp.MinimumPrice = $MinimumPrice",
				new
				{
					price.Id,
					price.RouteId,
					price.VehicleTypeId,
					price.PricePerKm,
					price.MinimumPrice
				});
		}

		private RoutePrice MapNodeToEntity(INode node)
		{
			return new RoutePrice
			{
				Id = node.Properties["Id"].As<int>(),
				RouteId = node.Properties["RouteId"].As<int>(),
				VehicleTypeId = node.Properties["VehicleTypeId"].As<int>(),
				PricePerKm = Convert.ToDecimal(node.Properties["PricePerKm"]),
				MinimumPrice = Convert.ToDecimal(node.Properties["MinimumPrice"])
			};
		}
	}
}
