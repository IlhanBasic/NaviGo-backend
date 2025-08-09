using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class DelayPenaltyNeo4jRepository
	{
		private readonly IDriver _driver;

		public DelayPenaltyNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(DelayPenalty penalty)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (dp:DelayPenalty {
                    Id: $id,
                    ShipmentId: $shipmentId,
                    CalculatedAt: $calculatedAt,
                    DelayHours: $delayHours,
                    PenaltyAmount: $penaltyAmount,
                    DelayPenaltiesStatus: $status
                })
                WITH dp
                MATCH (s:Shipment {Id: $shipmentId})
                CREATE (dp)-[:FOR_SHIPMENT]->(s)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = penalty.Id,
				["shipmentId"] = penalty.ShipmentId,
				["calculatedAt"] = penalty.CalculatedAt.ToString("o"),
				["delayHours"] = penalty.DelayHours,
				["penaltyAmount"] = penalty.PenaltyAmount,
				["status"] = (int)penalty.DelayPenaltiesStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (dp:DelayPenalty {Id: $id}) DETACH DELETE dp";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<DelayPenalty>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (dp:DelayPenalty) RETURN dp";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var penalties = new List<DelayPenalty>();

				foreach (var record in records)
				{
					var node = record["dp"].As<INode>();
					penalties.Add(MapNodeToDelayPenalty(node));
				}

				return penalties;
			});
		}

		public async Task<DelayPenalty?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (dp:DelayPenalty {Id: $id}) RETURN dp LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["dp"].As<INode>();

				return MapNodeToDelayPenalty(node);
			});
		}

		public async Task UpdateAsync(DelayPenalty penalty)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (dp:DelayPenalty {Id: $id})
                SET dp.ShipmentId = $shipmentId,
                    dp.CalculatedAt = $calculatedAt,
                    dp.DelayHours = $delayHours,
                    dp.PenaltyAmount = $penaltyAmount,
                    dp.DelayPenaltiesStatus = $status
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = penalty.Id,
				["shipmentId"] = penalty.ShipmentId,
				["calculatedAt"] = penalty.CalculatedAt.ToString("o"),
				["delayHours"] = penalty.DelayHours,
				["penaltyAmount"] = penalty.PenaltyAmount,
				["status"] = (int)penalty.DelayPenaltiesStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private DelayPenalty MapNodeToDelayPenalty(INode node)
		{
			return new DelayPenalty
			{
				Id = (int)(long)node.Properties["Id"],
				ShipmentId = (int)(long)node.Properties["ShipmentId"],
				CalculatedAt = DateTime.Parse((string)node.Properties["CalculatedAt"]),
				DelayHours = (int)(long)node.Properties["DelayHours"],
				PenaltyAmount = Convert.ToDecimal((double)node.Properties["PenaltyAmount"]),
				DelayPenaltiesStatus = (DelayPenaltyStatus)(int)(long)node.Properties["DelayPenaltiesStatus"]
			};
		}
	}
}
