using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class PickupChangeNeo4jRepository : IPickupChangeRepository
	{
		private readonly IDriver _driver;

		public PickupChangeNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(PickupChange change)
		{
			var id = await GetNextIdAsync("PickupChange");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (pc:PickupChange {
                    Id: $Id,
                    ShipmentId: $ShipmentId,
                    ClientId: $ClientId,
                    OldTime: datetime($OldTime),
                    NewTime: datetime($NewTime),
                    ChangeCount: $ChangeCount,
                    AdditionalFee: $AdditionalFee
                })",
					new
					{
						Id = id,
						change.ShipmentId,
						change.ClientId,
						OldTime = change.OldTime.ToString("o"),
						NewTime = change.NewTime.ToString("o"),
						change.ChangeCount,
						change.AdditionalFee,
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
				MATCH (pc:PickupChange {Id: $Id})
				DETACH DELETE pc",
				new { Id = id });
		}

		public async Task<IEnumerable<PickupChange>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (pc:PickupChange) RETURN pc");
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["pc"].As<INode>())).ToList();
		}

		public async Task<PickupChange?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (pc:PickupChange {Id: $Id})
				RETURN pc",
				new { Id = id });
			var records = await cursor.ToListAsync();
			if (records.Count == 0) return null;
			return MapNodeToEntity(records[0]["pc"].As<INode>());
		}

		public async Task UpdateAsync(PickupChange change)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (pc:PickupChange {Id: $Id})
				SET pc.ShipmentId = $ShipmentId,
					pc.ClientId = $ClientId,
					pc.OldTime = datetime($OldTime),
					pc.NewTime = datetime($NewTime),
					pc.ChangeCount = $ChangeCount,
					pc.AdditionalFee = $AdditionalFee",
				new
				{
					change.Id,
					change.ShipmentId,
					change.ClientId,
					OldTime = change.OldTime.ToString("o"),
					NewTime = change.NewTime.ToString("o"),
					change.ChangeCount,
					change.AdditionalFee
				});
		}

		private PickupChange MapNodeToEntity(INode node)
		{
			return new PickupChange
			{
				Id = node.Properties["Id"].As<int>(),
				ShipmentId = node.Properties["ShipmentId"].As<int>(),
				ClientId = node.Properties["ClientId"].As<int>(),
				OldTime = ConvertToDateTime(node.Properties["OldTime"]),
				NewTime = ConvertToDateTime(node.Properties["NewTime"]),
				ChangeCount = node.Properties["ChangeCount"].As<int>(),
				AdditionalFee = Convert.ToDecimal(node.Properties["AdditionalFee"]),
			};
		}

		// Helper metoda
		private DateTime ConvertToDateTime(object value)
		{
			return value switch
			{
				ZonedDateTime zdt => zdt.ToDateTimeOffset().UtcDateTime,
				LocalDateTime ldt => ldt.ToDateTime(),
				DateTime dt => dt,
				string s => DateTime.Parse(s),
				_ => throw new InvalidCastException($"Unexpected date type: {value.GetType()}")
			};
		}


		public async Task<PickupChange?> GetByShipmentAndClientAsync(int shipmentId, int clientId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
        MATCH (pc:PickupChange)
        WHERE pc.ShipmentId = $ShipmentId AND pc.ClientId = $ClientId
        RETURN pc
        LIMIT 1";

			var cursor = await session.RunAsync(query, new { ShipmentId = shipmentId, ClientId = clientId });
			var record = (await cursor.ToListAsync()).FirstOrDefault();

			if (record == null)
				return null;

			return MapNodeToEntity(record["pc"].As<INode>());
		}

	}
}
