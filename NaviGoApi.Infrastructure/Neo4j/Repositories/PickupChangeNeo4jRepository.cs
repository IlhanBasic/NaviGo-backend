using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class PickupChangeNeo4jRepository
	{
		private readonly IDriver _driver;

		public PickupChangeNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(PickupChange change)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (pc:PickupChange {
                    Id: $id,
                    ShipmentId: $shipmentId,
                    ClientId: $clientId,
                    OldTime: $oldTime,
                    NewTime: $newTime,
                    ChangeCount: $changeCount,
                    AdditionalFee: $additionalFee,
                    PickupChangesStatus: $status
                })
                WITH pc
                MATCH (s:Shipment {Id: $shipmentId}), (u:User {Id: $clientId})
                CREATE (pc)-[:FOR_SHIPMENT]->(s)
                CREATE (pc)-[:BY_CLIENT]->(u)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = change.Id,
				["shipmentId"] = change.ShipmentId,
				["clientId"] = change.ClientId,
				["oldTime"] = change.OldTime.ToString("o"),
				["newTime"] = change.NewTime.ToString("o"),
				["changeCount"] = change.ChangeCount,
				["additionalFee"] = change.AdditionalFee,
				["status"] = change.PickupChangesStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (pc:PickupChange {Id: $id}) DETACH DELETE pc";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<PickupChange>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (pc:PickupChange)-[:FOR_SHIPMENT]->(s:Shipment),
                      (pc)-[:BY_CLIENT]->(u:User)
                RETURN pc, s, u
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<PickupChange>();

				foreach (var record in records)
				{
					var nodePickupChange = record["pc"].As<INode>();
					var nodeShipment = record["s"].As<INode>();
					var nodeClient = record["u"].As<INode>();

					var pickupChange = MapNodeToPickupChange(nodePickupChange);
					pickupChange.Shipment = new Shipment { Id = (int)(long)nodeShipment.Properties["Id"] };
					pickupChange.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };

					list.Add(pickupChange);
				}

				return list;
			});
		}

		public async Task<PickupChange?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (pc:PickupChange {Id: $id})-[:FOR_SHIPMENT]->(s:Shipment),
                      (pc)-[:BY_CLIENT]->(u:User)
                RETURN pc, s, u
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();

				if (!hasRecord) return null;

				var record = cursor.Current;

				var nodePickupChange = record["pc"].As<INode>();
				var nodeShipment = record["s"].As<INode>();
				var nodeClient = record["u"].As<INode>();

				var pickupChange = MapNodeToPickupChange(nodePickupChange);
				pickupChange.Shipment = new Shipment { Id = (int)(long)nodeShipment.Properties["Id"] };
				pickupChange.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };

				return pickupChange;
			});
		}

		public async Task UpdateAsync(PickupChange change)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (pc:PickupChange {Id: $id})
                SET pc.ShipmentId = $shipmentId,
                    pc.ClientId = $clientId,
                    pc.OldTime = $oldTime,
                    pc.NewTime = $newTime,
                    pc.ChangeCount = $changeCount,
                    pc.AdditionalFee = $additionalFee,
                    pc.PickupChangesStatus = $status
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = change.Id,
				["shipmentId"] = change.ShipmentId,
				["clientId"] = change.ClientId,
				["oldTime"] = change.OldTime.ToString("o"),
				["newTime"] = change.NewTime.ToString("o"),
				["changeCount"] = change.ChangeCount,
				["additionalFee"] = change.AdditionalFee,
				["status"] = change.PickupChangesStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private PickupChange MapNodeToPickupChange(INode node)
		{
			return new PickupChange
			{
				Id = (int)(long)node.Properties["Id"],
				ShipmentId = (int)(long)node.Properties["ShipmentId"],
				ClientId = (int)(long)node.Properties["ClientId"],
				OldTime = DateTime.Parse((string)node.Properties["OldTime"]),
				NewTime = DateTime.Parse((string)node.Properties["NewTime"]),
				ChangeCount = (int)(long)node.Properties["ChangeCount"],
				AdditionalFee = Convert.ToDecimal((double)node.Properties["AdditionalFee"]),
				PickupChangesStatus = (int)(long)node.Properties["PickupChangesStatus"]
			};
		}
	}
}
