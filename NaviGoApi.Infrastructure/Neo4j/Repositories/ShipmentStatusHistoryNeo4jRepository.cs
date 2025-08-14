//using System;
//using System.Collections.Generic;
//using System.Threading.Tasks;
//using Neo4j.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;

//namespace NaviGoApi.Infrastructure.Neo4j.Repositories
//{
//	public class ShipmentStatusHistoryNeo4jRepository : IShipmentStatusHistoryRepository
//	{
//		private readonly IDriver _driver;

//		public ShipmentStatusHistoryNeo4jRepository(IDriver driver)
//		{
//			_driver = driver;
//		}

//private async Task<int> GetNextIdAsync(string entityName)
//{
//	var query = @"
//				MERGE (c:Counter { name: $entityName })
//				ON CREATE SET c.lastId = 1
//				ON MATCH SET c.lastId = c.lastId + 1
//				RETURN c.lastId as lastId
//			";

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

//		public async Task AddAsync(ShipmentStatusHistory history)
//		{
//			var id = await GetNextIdAsync("ShipmentStatusHistory");

//			var query = @"
//				CREATE (ssh:ShipmentStatusHistory {
//					id: $id,
//					shipmentId: $shipmentId,
//					shipmentStatus: $shipmentStatus,
//					changedAt: datetime($changedAt),
//					changedByUserId: $changedByUserId,
//					notes: $notes
//				})
//			";

//			var session = _driver.AsyncSession();
//			try
//			{
//				await session.RunAsync(query, new
//				{
//					id = id,
//					shipmentId = history.ShipmentId,
//					shipmentStatus = (int)history.ShipmentStatus,
//					changedAt = history.ChangedAt.ToString("o"),
//					changedByUserId = history.ChangedByUserId,
//					notes = history.Notes ?? ""
//				});
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task DeleteAsync(int id)
//		{
//			var query = @"
//				MATCH (ssh:ShipmentStatusHistory { id: $id })
//				DETACH DELETE ssh
//			";

//			var session = _driver.AsyncSession();
//			try
//			{
//				await session.RunAsync(query, new { id });
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
//		{
//			var query = @"
//				MATCH (ssh:ShipmentStatusHistory)
//				RETURN ssh
//			";

//			var session = _driver.AsyncSession();
//			try
//			{
//				var result = await session.RunAsync(query);
//				var list = new List<ShipmentStatusHistory>();

//				await result.ForEachAsync(record =>
//				{
//					var node = record["ssh"].As<INode>();
//					var history = new ShipmentStatusHistory
//					{
//						Id = Convert.ToInt32(node.Properties["id"]),
//						ShipmentId = Convert.ToInt32(node.Properties["shipmentId"]),
//						ShipmentStatus = (ShipmentStatus)Convert.ToInt32(node.Properties["shipmentStatus"]),
//						ChangedAt = DateTime.Parse(node.Properties["changedAt"].ToString()),
//						ChangedByUserId = Convert.ToInt32(node.Properties["changedByUserId"]),
//						Notes = node.Properties.ContainsKey("notes") ? node.Properties["notes"].ToString() : null
//					};
//					list.Add(history);
//				});
//				return list;
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
//		{
//			var query = @"
//        MATCH (ssh:ShipmentStatusHistory { id: $id })
//        RETURN ssh
//        LIMIT 1
//    ";

//			var session = _driver.AsyncSession();
//			try
//			{
//				var result = await session.RunAsync(query, new { id });
//				var record = await result.SingleAsync();

//				if (record == null)
//					return null;

//				var node = record["ssh"].As<INode>();

//				return new ShipmentStatusHistory
//				{
//					Id = Convert.ToInt32(node.Properties["id"]),
//					ShipmentId = Convert.ToInt32(node.Properties["shipmentId"]),
//					ShipmentStatus = (ShipmentStatus)Convert.ToInt32(node.Properties["shipmentStatus"]),
//					ChangedAt = DateTime.Parse(node.Properties["changedAt"].ToString()),
//					ChangedByUserId = Convert.ToInt32(node.Properties["changedByUserId"]),
//					Notes = node.Properties.ContainsKey("notes") ? node.Properties["notes"].ToString() : null
//				};
//			}
//			catch (InvalidOperationException)
//			{
//				// SingleAsync baca InvalidOperationException ako nema rezultata
//				return null;
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task UpdateAsync(ShipmentStatusHistory history)
//		{
//			var query = @"
//				MATCH (ssh:ShipmentStatusHistory { id: $id })
//				SET
//					ssh.shipmentId = $shipmentId,
//					ssh.shipmentStatus = $shipmentStatus,
//					ssh.changedAt = datetime($changedAt),
//					ssh.changedByUserId = $changedByUserId,
//					ssh.notes = $notes
//			";

//			var session = _driver.AsyncSession();
//			try
//			{
//				await session.RunAsync(query, new
//				{
//					id = history.Id,
//					shipmentId = history.ShipmentId,
//					shipmentStatus = (int)history.ShipmentStatus,
//					changedAt = history.ChangedAt.ToString("o"),
//					changedByUserId = history.ChangedByUserId,
//					notes = history.Notes ?? ""
//				});
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
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
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentStatusHistoryNeo4jRepository : IShipmentStatusHistoryRepository
	{
		private readonly IDriver _driver;

		public ShipmentStatusHistoryNeo4jRepository(IDriver driver)
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
		public async Task AddAsync(ShipmentStatusHistory history)
		{
			var id = await GetNextIdAsync("ShipmentStatusHistory");
			var query = @"
                CREATE (h:ShipmentStatusHistory {
                    Id: $id,
                    ShipmentId: $shipmentId,
                    ShipmentStatus: $status,
                    ChangedAt: $changedAt,
                    ChangedByUserId: $userId,
                    Notes: $notes
                })";

			var session = _driver.AsyncSession();
			await session.RunAsync(query, new
			{
				id = id,
				shipmentId = history.ShipmentId,
				status = (int)history.ShipmentStatus,
				changedAt = history.ChangedAt,
				userId = history.ChangedByUserId,
				notes = history.Notes
			});
			await session.CloseAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"MATCH (h:ShipmentStatusHistory {Id: $id}) DETACH DELETE h";
			var session = _driver.AsyncSession();
			await session.RunAsync(query, new { id });
			await session.CloseAsync();
		}

		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
		{
			var query = @"MATCH (h:ShipmentStatusHistory) RETURN h";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query);

			var list = new List<ShipmentStatusHistory>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToHistory(result.Current["h"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			var query = @"MATCH (h:ShipmentStatusHistory {Id: $id}) RETURN h LIMIT 1";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { id });

			if (!await result.FetchAsync())
			{
				await session.CloseAsync();
				return null;
			}

			var node = result.Current["h"].As<INode>();
			await session.CloseAsync();
			return MapNodeToHistory(node);
		}

		public async Task<ShipmentStatusHistory?> GetLastStatusForShipmentAsync(int shipmentId)
		{
			var query = @"
                MATCH (h:ShipmentStatusHistory {ShipmentId: $shipmentId})
                RETURN h
                ORDER BY h.ChangedAt DESC
                LIMIT 1";

			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { shipmentId });

			if (!await result.FetchAsync())
			{
				await session.CloseAsync();
				return null;
			}

			var node = result.Current["h"].As<INode>();
			await session.CloseAsync();
			return MapNodeToHistory(node);
		}

		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			var query = @"
                MATCH (h:ShipmentStatusHistory {Id: $id})
                SET h.ShipmentId = $shipmentId,
                    h.ShipmentStatus = $status,
                    h.ChangedAt = $changedAt,
                    h.ChangedByUserId = $userId,
                    h.Notes = $notes";

			var session = _driver.AsyncSession();
			await session.RunAsync(query, new
			{
				id = history.Id,
				shipmentId = history.ShipmentId,
				status = (int)history.ShipmentStatus,
				changedAt = history.ChangedAt,
				userId = history.ChangedByUserId,
				notes = history.Notes
			});
			await session.CloseAsync();
		}

		private ShipmentStatusHistory MapNodeToHistory(INode node)
		{
			return new ShipmentStatusHistory
			{
				Id = node.Properties["Id"].As<int>(),
				ShipmentId = node.Properties["ShipmentId"].As<int>(),
				ShipmentStatus = (ShipmentStatus)(int)node.Properties["ShipmentStatus"].As<long>(),
				ChangedAt = ConvertZonedDateTime(node.Properties["ChangedAt"]),
				ChangedByUserId = node.Properties["ChangedByUserId"].As<int>(),
				Notes = node.Properties.ContainsKey("Notes") ? node.Properties["Notes"].As<string>() : null
			};
		}

		// Helper metoda za konverziju
		private DateTime ConvertZonedDateTime(object value)
		{
			return value switch
			{
				ZonedDateTime zdt => zdt.ToDateTimeOffset().UtcDateTime,
				LocalDateTime ldt => ldt.ToDateTime(),
				DateTime dt => dt,
				_ => throw new InvalidCastException($"Unexpected date type: {value.GetType()}")
			};
		}

	}
}
