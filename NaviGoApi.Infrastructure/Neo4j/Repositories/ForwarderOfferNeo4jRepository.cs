using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ForwarderOfferNeo4jRepository : IForwarderOfferRepository
	{
		private readonly IDriver _driver;

		public ForwarderOfferNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(ForwarderOffer offer)
		{
			var id = await GetNextIdAsync("ForwarderOffer");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (o:ForwarderOffer {
                    Id: $Id,
                    RouteId: $RouteId,
                    ForwarderId: $ForwarderId,
                    CommissionRate: $CommissionRate,
                    ForwarderOfferStatus: $ForwarderOfferStatus,
                    RejectionReason: $RejectionReason,
                    CreatedAt: $CreatedAt,
                    ExpiresAt: $ExpiresAt,
                    DiscountRate: $DiscountRate
                })",
					new
					{
						Id = id,
						offer.RouteId,
						offer.ForwarderId,
						offer.CommissionRate,
						ForwarderOfferStatus = (int)offer.ForwarderOfferStatus,
						offer.RejectionReason,
						offer.CreatedAt,
						offer.ExpiresAt,
						offer.DiscountRate
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(ForwarderOffer offer)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (o:ForwarderOffer {Id: $Id})
                DETACH DELETE o",
				new { offer.Id });
		}

		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			await using var session = _driver.AsyncSession();
			var now = DateTime.UtcNow;

			var cursor = await session.RunAsync(@"
                MATCH (o:ForwarderOffer)
                WHERE o.ForwarderOfferStatus = $Status AND o.ExpiresAt > $Now
                RETURN o",
				new { Status = (int)ForwarderOfferStatus.Pending, Now = now });

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["o"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (o:ForwarderOffer) RETURN o");

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["o"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (o:ForwarderOffer {ForwarderId: $ForwarderId})
                RETURN o",
				new { ForwarderId = forwarderId });

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["o"].As<INode>())).ToList();
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (o:ForwarderOffer {Id: $Id})
                RETURN o",
				new { Id = id });

			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;

			return MapNodeToEntity(records[0]["o"].As<INode>());
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (o:ForwarderOffer {RouteId: $RouteId})
                RETURN o",
				new { RouteId = routeId });

			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["o"].As<INode>())).ToList();
		}

		public async Task UpdateAsync(ForwarderOffer offer)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (o:ForwarderOffer {Id: $Id})
                SET o.RouteId = $RouteId,
                    o.ForwarderId = $ForwarderId,
                    o.CommissionRate = $CommissionRate,
                    o.ForwarderOfferStatus = $ForwarderOfferStatus,
                    o.RejectionReason = $RejectionReason,
                    o.CreatedAt = $CreatedAt,
                    o.ExpiresAt = $ExpiresAt,
                    o.DiscountRate = $DiscountRate",
				new
				{
					offer.Id,
					offer.RouteId,
					offer.ForwarderId,
					offer.CommissionRate,
					ForwarderOfferStatus = (int)offer.ForwarderOfferStatus,
					offer.RejectionReason,
					offer.CreatedAt,
					offer.ExpiresAt,
					offer.DiscountRate
				});
		}

		private ForwarderOffer MapNodeToEntity(INode node)
		{
			return new ForwarderOffer
			{
				Id = node.Properties["Id"].As<int>(),
				RouteId = node.Properties["RouteId"].As<int>(),
				ForwarderId = node.Properties["ForwarderId"].As<int>(),
				CommissionRate = node.Properties["CommissionRate"].As<decimal>(),
				ForwarderOfferStatus = (ForwarderOfferStatus)node.Properties["ForwarderOfferStatus"].As<int>(),
				RejectionReason = node.Properties.ContainsKey("RejectionReason") ? node.Properties["RejectionReason"].As<string?>() : null,
				CreatedAt = node.Properties["CreatedAt"].As<DateTime>(),
				ExpiresAt = node.Properties["ExpiresAt"].As<DateTime>(),
				DiscountRate = node.Properties["DiscountRate"].As<decimal>()
			};
		}
	}
}
