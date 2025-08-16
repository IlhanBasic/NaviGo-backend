
using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ForwarderOfferNeo4jRepository : IForwarderOfferRepository
	{
		private readonly IDriver _driver;

		public ForwarderOfferNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(ForwarderOffer offer)
		{
			var id = await GetNextIdAsync("ForwarderOffer");

			var query = @"
                CREATE (f:ForwarderOffer {
                    Id: $Id,
                    RouteId: $RouteId,
                    ForwarderId: $ForwarderId,
                    CommissionRate: $CommissionRate,
                    ForwarderOfferStatus: $ForwarderOfferStatus,
                    RejectionReason: $RejectionReason,
                    CreatedAt: $CreatedAt,
                    ExpiresAt: $ExpiresAt,
                    DiscountRate: $DiscountRate
                })
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
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
			var query = @"MATCH (f:ForwarderOffer { Id: $Id }) DETACH DELETE f";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { offer.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			var query = @"MATCH (f:ForwarderOffer) RETURN f";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var records = await result.ToListAsync();
				return records.Select(r => MapForwarderOfferNode(r["f"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<ForwarderOffer>> GetActiveOffersAsync()
		{
			var now = DateTime.UtcNow;
			var query = @"
                MATCH (f:ForwarderOffer)
                WHERE f.ForwarderOfferStatus = $Pending AND f.ExpiresAt > $Now
                RETURN f
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Pending = (int)ForwarderOfferStatus.Pending, Now = now });
				var records = await result.ToListAsync();
				return records.Select(r => MapForwarderOfferNode(r["f"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			var query = @"MATCH (f:ForwarderOffer { Id: $Id }) RETURN f";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				return MapForwarderOfferNode(record["f"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			var query = @"MATCH (f:ForwarderOffer { ForwarderId: $ForwarderId }) RETURN f";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { ForwarderId = forwarderId });
				var records = await result.ToListAsync();
				return records.Select(r => MapForwarderOfferNode(r["f"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			var query = @"MATCH (f:ForwarderOffer { RouteId: $RouteId }) RETURN f";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { RouteId = routeId });
				var records = await result.ToListAsync();
				return records.Select(r => MapForwarderOfferNode(r["f"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public Task UpdateAsync(ForwarderOffer offer)
		{
			var query = @"
                MATCH (f:ForwarderOffer { Id: $Id })
                SET f.RouteId = $RouteId,
                    f.ForwarderId = $ForwarderId,
                    f.CommissionRate = $CommissionRate,
                    f.ForwarderOfferStatus = $ForwarderOfferStatus,
                    f.RejectionReason = $RejectionReason,
                    f.CreatedAt = $CreatedAt,
                    f.ExpiresAt = $ExpiresAt,
                    f.DiscountRate = $DiscountRate
            ";

			var session = _driver.AsyncSession();
			return session.RunAsync(query, new
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

		private ForwarderOffer MapForwarderOfferNode(INode node)
		{
			DateTime ConvertNeo4jDateTime(string key)
			{
				if (!node.Properties.ContainsKey(key) || node.Properties[key] == null)
					return default;

				var zoned = node.Properties[key].As<ZonedDateTime>();
				return zoned.ToDateTimeOffset().UtcDateTime;
			}

			return new ForwarderOffer
			{
				Id = node["Id"].As<int>(),
				RouteId = node["RouteId"].As<int>(),
				ForwarderId = node["ForwarderId"].As<int>(),
				CommissionRate = node["CommissionRate"].As<decimal>(),
				ForwarderOfferStatus = (ForwarderOfferStatus)node["ForwarderOfferStatus"].As<int>(),
				RejectionReason = node.Properties.ContainsKey("RejectionReason") ? node["RejectionReason"].As<string?>() : null,
				CreatedAt = ConvertNeo4jDateTime("CreatedAt"),
				ExpiresAt = ConvertNeo4jDateTime("ExpiresAt"),
				DiscountRate = node["DiscountRate"].As<decimal>()
			};
		}

		public Task<IEnumerable<ForwarderOffer>> GetAllAsync(ForwarderOfferSearchDto forwarderOfferSearch)
		{
			throw new NotImplementedException();
		}
	}
}
