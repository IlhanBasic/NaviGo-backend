using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class DelayPenaltyNeo4jRepository : IDelayPenaltyRepository
	{
		private readonly IDriver _driver;

		public DelayPenaltyNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(DelayPenalty penalty)
		{
			var id = await GetNextIdAsync("DelayPenalty");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (p:DelayPenalty {
                    Id: $Id,
                    ShipmentId: $ShipmentId,
                    CalculatedAt: $CalculatedAt,
                    DelayHours: $DelayHours,
                    PenaltyAmount: $PenaltyAmount,
                    DelayPenaltiesStatus: $DelayPenaltiesStatus
                })",
					new
					{
						Id = id,
						penalty.ShipmentId,
						penalty.CalculatedAt,
						penalty.DelayHours,
						penalty.PenaltyAmount,
						DelayPenaltiesStatus = (int)penalty.DelayPenaltiesStatus
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (p:DelayPenalty {Id: $Id})
                DETACH DELETE p",
				new { Id = id });
		}

		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (p:DelayPenalty)
                RETURN p");

			var records = await cursor.ToListAsync();
			var list = new List<DelayPenalty>();

			foreach (var record in records)
			{
				var node = record["p"].As<INode>();
				list.Add(MapNodeToEntity(node));
			}

			return list;
		}

		public async Task<DelayPenalty?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
                MATCH (p:DelayPenalty {Id: $Id})
                RETURN p",
				new { Id = id });

			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;

			var node = records[0]["p"].As<INode>();
			return MapNodeToEntity(node);
		}

		public async Task UpdateAsync(DelayPenalty penalty)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
                MATCH (p:DelayPenalty {Id: $Id})
                SET p.ShipmentId = $ShipmentId,
                    p.CalculatedAt = $CalculatedAt,
                    p.DelayHours = $DelayHours,
                    p.PenaltyAmount = $PenaltyAmount,
                    p.DelayPenaltiesStatus = $DelayPenaltiesStatus",
				new
				{
					Id = penalty.Id,
					ShipmentId = penalty.ShipmentId,
					CalculatedAt = penalty.CalculatedAt,
					DelayHours = penalty.DelayHours,
					PenaltyAmount = penalty.PenaltyAmount,
					DelayPenaltiesStatus = (int)penalty.DelayPenaltiesStatus
				});
		}

		private DelayPenalty MapNodeToEntity(INode node)
		{
			return new DelayPenalty
			{
				Id = node.Properties["Id"].As<int>(),
				ShipmentId = node.Properties["ShipmentId"].As<int>(),
				CalculatedAt = node.Properties["CalculatedAt"].As<DateTime>(),
				DelayHours = node.Properties["DelayHours"].As<int>(),
				PenaltyAmount = node.Properties["PenaltyAmount"].As<decimal>(),
				DelayPenaltiesStatus = (DelayPenaltyStatus)node.Properties["DelayPenaltiesStatus"].As<int>()
			};
		}

		public Task<IEnumerable<DelayPenalty>> GetByContractIdAsync(int contractId)
		{
			throw new NotImplementedException();
		}
	}
}
