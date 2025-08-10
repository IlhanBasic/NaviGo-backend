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

		public async Task AddAsync(PickupChange change)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				CREATE (pc:PickupChange {
					Id: $Id,
					ShipmentId: $ShipmentId,
					ClientId: $ClientId,
					OldTime: datetime($OldTime),
					NewTime: datetime($NewTime),
					ChangeCount: $ChangeCount,
					AdditionalFee: $AdditionalFee,
					PickupChangesStatus: $PickupChangesStatus
				})",
				new
				{
					change.Id,
					change.ShipmentId,
					change.ClientId,
					OldTime = change.OldTime.ToString("o"),
					NewTime = change.NewTime.ToString("o"),
					change.ChangeCount,
					change.AdditionalFee,
					change.PickupChangesStatus
				});
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
					pc.AdditionalFee = $AdditionalFee,
					pc.PickupChangesStatus = $PickupChangesStatus",
				new
				{
					change.Id,
					change.ShipmentId,
					change.ClientId,
					OldTime = change.OldTime.ToString("o"),
					NewTime = change.NewTime.ToString("o"),
					change.ChangeCount,
					change.AdditionalFee,
					change.PickupChangesStatus
				});
		}

		private PickupChange MapNodeToEntity(INode node)
		{
			return new PickupChange
			{
				Id = node.Properties["Id"].As<int>(),
				ShipmentId = node.Properties["ShipmentId"].As<int>(),
				ClientId = node.Properties["ClientId"].As<int>(),
				OldTime = DateTime.Parse(node.Properties["OldTime"].As<string>()),
				NewTime = DateTime.Parse(node.Properties["NewTime"].As<string>()),
				ChangeCount = node.Properties["ChangeCount"].As<int>(),
				AdditionalFee = Convert.ToDecimal(node.Properties["AdditionalFee"]),
				PickupChangesStatus = node.Properties["PickupChangesStatus"].As<int>()
			};
		}
	}
}
