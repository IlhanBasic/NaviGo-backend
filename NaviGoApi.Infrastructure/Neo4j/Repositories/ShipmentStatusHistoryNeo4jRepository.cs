using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class ShipmentStatusHistoryNeo4jRepository
	{
		private readonly IDriver _driver;

		public ShipmentStatusHistoryNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(ShipmentStatusHistory history)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (s:Shipment {Id: $shipmentId})
                MATCH (u:User {Id: $userId})
                CREATE (h:ShipmentStatusHistory {
                    Id: $id,
                    ShipmentStatus: $status,
                    ChangedAt: datetime($changedAt),
                    Notes: $notes
                })
                CREATE (h)-[:BELONGS_TO]->(s)
                CREATE (h)-[:CHANGED_BY]->(u)";

			var parameters = new Dictionary<string, object>
			{
				["id"] = history.Id,
				["shipmentId"] = history.ShipmentId,
				["status"] = (int)history.ShipmentStatus,
				["changedAt"] = history.ChangedAt.ToString("o"),
				["userId"] = history.ChangedByUserId,
				["notes"] = history.Notes ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (h:ShipmentStatusHistory {Id: $id})
                DETACH DELETE h";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<ShipmentStatusHistory>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (h:ShipmentStatusHistory)-[:BELONGS_TO]->(s:Shipment)
                OPTIONAL MATCH (h)-[:CHANGED_BY]->(u:User)
                RETURN h, s, u";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<ShipmentStatusHistory>();

				foreach (var record in records)
				{
					var hNode = record["h"].As<INode>();
					var sNode = record["s"].As<INode>();
					var uNode = record["u"] as INode;

					var history = new ShipmentStatusHistory
					{
						Id = (int)(long)hNode.Properties["Id"],
						ShipmentId = (int)(long)hNode.Properties["ShipmentId"], // You may want to store ShipmentId in node properties as well
						ShipmentStatus = (ShipmentStatus)(int)(long)hNode.Properties["ShipmentStatus"],
						ChangedAt = DateTime.Parse((string)hNode.Properties["ChangedAt"]),
						Notes = hNode.Properties.ContainsKey("Notes") ? (string)hNode.Properties["Notes"] : null,
						Shipment = new Shipment
						{
							Id = (int)(long)sNode.Properties["Id"],
							// Optionally map more Shipment properties here
						},
						ChangedByUser = uNode != null ? new User
						{
							Id = (int)(long)uNode.Properties["Id"],
							// Optionally map more User properties here
						} : null
					};

					list.Add(history);
				}

				return list;
			});
		}

		public async Task<ShipmentStatusHistory?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (h:ShipmentStatusHistory {Id: $id})-[:BELONGS_TO]->(s:Shipment)
                OPTIONAL MATCH (h)-[:CHANGED_BY]->(u:User)
                RETURN h, s, u
                LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				if (!await cursor.FetchAsync()) return null;

				var hNode = cursor.Current["h"].As<INode>();
				var sNode = cursor.Current["s"].As<INode>();
				var uNode = cursor.Current["u"] as INode;

				return new ShipmentStatusHistory
				{
					Id = (int)(long)hNode.Properties["Id"],
					ShipmentId = (int)(long)hNode.Properties["ShipmentId"], // Store ShipmentId on node to retrieve easily
					ShipmentStatus = (ShipmentStatus)(int)(long)hNode.Properties["ShipmentStatus"],
					ChangedAt = DateTime.Parse((string)hNode.Properties["ChangedAt"]),
					Notes = hNode.Properties.ContainsKey("Notes") ? (string)hNode.Properties["Notes"] : null,
					Shipment = new Shipment
					{
						Id = (int)(long)sNode.Properties["Id"],
						// Map more if needed
					},
					ChangedByUser = uNode != null ? new User
					{
						Id = (int)(long)uNode.Properties["Id"],
						// Map more if needed
					} : null
				};
			});
		}

		public async Task UpdateAsync(ShipmentStatusHistory history)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (h:ShipmentStatusHistory {Id: $id})
                SET h.ShipmentStatus = $status,
                    h.ChangedAt = datetime($changedAt),
                    h.Notes = $notes";

			var parameters = new Dictionary<string, object>
			{
				["id"] = history.Id,
				["status"] = (int)history.ShipmentStatus,
				["changedAt"] = history.ChangedAt.ToString("o"),
				["notes"] = history.Notes ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}
	}
}
