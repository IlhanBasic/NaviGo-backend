using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ForwarderOfferNeo4jRepository
	{
		private readonly IDriver _driver;

		public ForwarderOfferNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(ForwarderOffer offer)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (fo:ForwarderOffer {
                    Id: $id,
                    RouteId: $routeId,
                    ForwarderId: $forwarderId,
                    CommissionRate: $commissionRate,
                    ForwarderOfferStatus: $status,
                    RejectionReason: $rejectionReason,
                    CreatedAt: $createdAt,
                    ExpiresAt: $expiresAt,
                    DiscountRate: $discountRate
                })
                WITH fo
                MATCH (r:Route {Id: $routeId}), (f:Company {Id: $forwarderId})
                CREATE (fo)-[:FOR_ROUTE]->(r)
                CREATE (fo)-[:BY_FORWARDER]->(f)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = offer.Id,
				["routeId"] = offer.RouteId,
				["forwarderId"] = offer.ForwarderId,
				["commissionRate"] = offer.CommissionRate,
				["status"] = (int)offer.ForwarderOfferStatus,
				["rejectionReason"] = offer.RejectionReason ?? "",
				["createdAt"] = offer.CreatedAt.ToString("o"),
				["expiresAt"] = offer.ExpiresAt.ToString("o"),
				["discountRate"] = offer.DiscountRate
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (fo:ForwarderOffer {Id: $id}) DETACH DELETE fo";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<ForwarderOffer>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (fo:ForwarderOffer)-[:FOR_ROUTE]->(r:Route),
                      (fo)-[:BY_FORWARDER]->(f:Company)
                RETURN fo, r, f
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var offers = new List<ForwarderOffer>();

				foreach (var record in records)
				{
					var nodeOffer = record["fo"].As<INode>();
					var nodeRoute = record["r"].As<INode>();
					var nodeForwarder = record["f"].As<INode>();

					var offer = MapNodeToForwarderOffer(nodeOffer);
					offer.Route = new Route { Id = (int)(long)nodeRoute.Properties["Id"] };
					offer.Forwarder = new Company { Id = (int)(long)nodeForwarder.Properties["Id"] };

					offers.Add(offer);
				}

				return offers;
			});
		}

		public async Task<ForwarderOffer?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (fo:ForwarderOffer {Id: $id})-[:FOR_ROUTE]->(r:Route),
                      (fo)-[:BY_FORWARDER]->(f:Company)
                RETURN fo, r, f
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();

				if (!hasRecord) return null;

				var record = cursor.Current;

				var nodeOffer = record["fo"].As<INode>();
				var nodeRoute = record["r"].As<INode>();
				var nodeForwarder = record["f"].As<INode>();

				var offer = MapNodeToForwarderOffer(nodeOffer);
				offer.Route = new Route { Id = (int)(long)nodeRoute.Properties["Id"] };
				offer.Forwarder = new Company { Id = (int)(long)nodeForwarder.Properties["Id"] };

				return offer;
			});
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByForwarderIdAsync(int forwarderId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (fo:ForwarderOffer {ForwarderId: $forwarderId})-[:FOR_ROUTE]->(r:Route),
                      (fo)-[:BY_FORWARDER]->(f:Company)
                RETURN fo, r, f
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { forwarderId });
				var records = await cursor.ToListAsync();

				var offers = new List<ForwarderOffer>();

				foreach (var record in records)
				{
					var nodeOffer = record["fo"].As<INode>();
					var nodeRoute = record["r"].As<INode>();
					var nodeForwarder = record["f"].As<INode>();

					var offer = MapNodeToForwarderOffer(nodeOffer);
					offer.Route = new Route { Id = (int)(long)nodeRoute.Properties["Id"] };
					offer.Forwarder = new Company { Id = (int)(long)nodeForwarder.Properties["Id"] };

					offers.Add(offer);
				}

				return offers;
			});
		}

		public async Task<IEnumerable<ForwarderOffer>> GetByRouteIdAsync(int routeId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (fo:ForwarderOffer {RouteId: $routeId})-[:FOR_ROUTE]->(r:Route),
                      (fo)-[:BY_FORWARDER]->(f:Company)
                RETURN fo, r, f
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { routeId });
				var records = await cursor.ToListAsync();

				var offers = new List<ForwarderOffer>();

				foreach (var record in records)
				{
					var nodeOffer = record["fo"].As<INode>();
					var nodeRoute = record["r"].As<INode>();
					var nodeForwarder = record["f"].As<INode>();

					var offer = MapNodeToForwarderOffer(nodeOffer);
					offer.Route = new Route { Id = (int)(long)nodeRoute.Properties["Id"] };
					offer.Forwarder = new Company { Id = (int)(long)nodeForwarder.Properties["Id"] };

					offers.Add(offer);
				}

				return offers;
			});
		}

		public async Task UpdateAsync(ForwarderOffer offer)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (fo:ForwarderOffer {Id: $id})
                SET fo.RouteId = $routeId,
                    fo.ForwarderId = $forwarderId,
                    fo.CommissionRate = $commissionRate,
                    fo.ForwarderOfferStatus = $status,
                    fo.RejectionReason = $rejectionReason,
                    fo.CreatedAt = $createdAt,
                    fo.ExpiresAt = $expiresAt,
                    fo.DiscountRate = $discountRate
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = offer.Id,
				["routeId"] = offer.RouteId,
				["forwarderId"] = offer.ForwarderId,
				["commissionRate"] = offer.CommissionRate,
				["status"] = (int)offer.ForwarderOfferStatus,
				["rejectionReason"] = offer.RejectionReason ?? "",
				["createdAt"] = offer.CreatedAt.ToString("o"),
				["expiresAt"] = offer.ExpiresAt.ToString("o"),
				["discountRate"] = offer.DiscountRate
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private ForwarderOffer MapNodeToForwarderOffer(INode node)
		{
			return new ForwarderOffer
			{
				Id = (int)(long)node.Properties["Id"],
				RouteId = (int)(long)node.Properties["RouteId"],
				ForwarderId = (int)(long)node.Properties["ForwarderId"],
				CommissionRate = Convert.ToDecimal((double)node.Properties["CommissionRate"]),
				ForwarderOfferStatus = (ForwarderOfferStatus)(int)(long)node.Properties["ForwarderOfferStatus"],
				RejectionReason = node.Properties.ContainsKey("RejectionReason") ? (string)node.Properties["RejectionReason"] : null,
				CreatedAt = DateTime.Parse((string)node.Properties["CreatedAt"]),
				ExpiresAt = DateTime.Parse((string)node.Properties["ExpiresAt"]),
				DiscountRate = Convert.ToDecimal((double)node.Properties["DiscountRate"])
			};
		}
	}
}
