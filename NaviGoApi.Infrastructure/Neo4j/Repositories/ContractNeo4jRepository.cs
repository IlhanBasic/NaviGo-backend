using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ContractNeo4jRepository : IContractRepository
	{
		private readonly IDriver _driver;

		public ContractNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Contract contract)
		{
			var id = await GetNextIdAsync("Contract");

			var query = @"
            CREATE (c:Contract {
                Id: $Id,
                ClientId: $ClientId,
                ForwarderId: $ForwarderId,
                RouteId: $RouteId,
                ContractNumber: $ContractNumber,
                ContractDate: datetime($ContractDate),
                Terms: $Terms,
                ContractStatus: $ContractStatus,
                PenaltyRatePerHour: $PenaltyRatePerHour,
                MaxPenaltyPercent: $MaxPenaltyPercent,
                ValidUntil: datetime($ValidUntil),
                SignedDate: CASE WHEN $SignedDate IS NULL THEN NULL ELSE datetime($SignedDate) END
            })
        ";

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					contract.ClientId,
					contract.ForwarderId,
					contract.RouteId,
					contract.ContractNumber,
					ContractDate = contract.ContractDate,
					contract.Terms,
					ContractStatus = (int)contract.ContractStatus,
					contract.PenaltyRatePerHour,
					contract.MaxPenaltyPercent,
					ValidUntil = contract.ValidUntil,
					SignedDate = contract.SignedDate
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Contract contract)
		{
			var query = "MATCH (c:Contract { Id: $Id }) DETACH DELETE c";

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { contract.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			var query = @"
				MATCH (c:Contract)
				OPTIONAL MATCH (c)-[:HAS_CLIENT]->(client:User)
				OPTIONAL MATCH (c)-[:HAS_FORWARDER]->(forwarder:Company)
				OPTIONAL MATCH (c)-[:HAS_ROUTE]->(route:Route)
				RETURN c, client, forwarder, route
			";

			await using var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var records = await result.ToListAsync();

				return records.Select(r =>
				{
					var cNode = r["c"].As<INode>();

					return new Contract
					{
						Id = cNode["Id"].As<int>(),
						ClientId = cNode["ClientId"].As<int>(),
						ForwarderId = cNode["ForwarderId"].As<int>(),
						RouteId = cNode["RouteId"].As<int>(),
						ContractNumber = cNode["ContractNumber"].As<string>(),
						ContractDate = cNode["ContractDate"].As<DateTime>(),
						Terms = cNode["Terms"].As<string>(),
						ContractStatus = (ContractStatus)cNode["ContractStatus"].As<int>(),
						PenaltyRatePerHour = cNode["PenaltyRatePerHour"].As<decimal>(),
						MaxPenaltyPercent = cNode["MaxPenaltyPercent"].As<decimal>(),
						ValidUntil = cNode["ValidUntil"].As<DateTime>(),
						SignedDate = cNode.Properties.ContainsKey("SignedDate") ? cNode["SignedDate"].As<DateTime?>() : null
					};
				}).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			var query = @"
				MATCH (c:Contract { ClientId: $ClientId })
				RETURN c
			";

			await using var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { ClientId = clientId });
				var records = await result.ToListAsync();

				return records.Select(r =>
				{
					var cNode = r["c"].As<INode>();
					return new Contract
					{
						Id = cNode["Id"].As<int>(),
						ClientId = cNode["ClientId"].As<int>(),
						ForwarderId = cNode["ForwarderId"].As<int>(),
						RouteId = cNode["RouteId"].As<int>(),
						ContractNumber = cNode["ContractNumber"].As<string>(),
						ContractDate = cNode["ContractDate"].As<DateTime>(),
						Terms = cNode["Terms"].As<string>(),
						ContractStatus = (ContractStatus)cNode["ContractStatus"].As<int>(),
						PenaltyRatePerHour = cNode["PenaltyRatePerHour"].As<decimal>(),
						MaxPenaltyPercent = cNode["MaxPenaltyPercent"].As<decimal>(),
						ValidUntil = cNode["ValidUntil"].As<DateTime>(),
						SignedDate = cNode.Properties.ContainsKey("SignedDate") ? cNode["SignedDate"].As<DateTime?>() : null
					};
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			var query = @"
				MATCH (c:Contract { ForwarderId: $ForwarderId })
				RETURN c
			";

			await using var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { ForwarderId = forwarderId });
				var records = await result.ToListAsync();

				return records.Select(r =>
				{
					var cNode = r["c"].As<INode>();
					return new Contract
					{
						Id = cNode["Id"].As<int>(),
						ClientId = cNode["ClientId"].As<int>(),
						ForwarderId = cNode["ForwarderId"].As<int>(),
						RouteId = cNode["RouteId"].As<int>(),
						ContractNumber = cNode["ContractNumber"].As<string>(),
						ContractDate = cNode["ContractDate"].As<DateTime>(),
						Terms = cNode["Terms"].As<string>(),
						ContractStatus = (ContractStatus)cNode["ContractStatus"].As<int>(),
						PenaltyRatePerHour = cNode["PenaltyRatePerHour"].As<decimal>(),
						MaxPenaltyPercent = cNode["MaxPenaltyPercent"].As<decimal>(),
						ValidUntil = cNode["ValidUntil"].As<DateTime>(),
						SignedDate = cNode.Properties.ContainsKey("SignedDate") ? cNode["SignedDate"].As<DateTime?>() : null
					};
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			var query = "MATCH (c:Contract { Id: $Id }) RETURN c";

			await using var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var records = await result.ToListAsync();
				var record = records.FirstOrDefault();
				if (record == null) return null;

				var cNode = record["c"].As<INode>();
				return new Contract
				{
					Id = cNode["Id"].As<int>(),
					ClientId = cNode["ClientId"].As<int>(),
					ForwarderId = cNode["ForwarderId"].As<int>(),
					RouteId = cNode["RouteId"].As<int>(),
					ContractNumber = cNode["ContractNumber"].As<string>(),
					ContractDate = cNode["ContractDate"].As<DateTime>(),
					Terms = cNode["Terms"].As<string>(),
					ContractStatus = (ContractStatus)cNode["ContractStatus"].As<int>(),
					PenaltyRatePerHour = cNode["PenaltyRatePerHour"].As<decimal>(),
					MaxPenaltyPercent = cNode["MaxPenaltyPercent"].As<decimal>(),
					ValidUntil = cNode["ValidUntil"].As<DateTime>(),
					SignedDate = cNode.Properties.ContainsKey("SignedDate") ? cNode["SignedDate"].As<DateTime?>() : null
				};
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Contract contract)
		{
			var query = @"
				MATCH (c:Contract { Id: $Id })
				SET c.ClientId = $ClientId,
					c.ForwarderId = $ForwarderId,
					c.RouteId = $RouteId,
					c.ContractNumber = $ContractNumber,
					c.ContractDate = datetime($ContractDate),
					c.Terms = $Terms,
					c.ContractStatus = $ContractStatus,
					c.PenaltyRatePerHour = $PenaltyRatePerHour,
					c.MaxPenaltyPercent = $MaxPenaltyPercent,
					c.ValidUntil = datetime($ValidUntil),
					c.SignedDate = CASE WHEN $SignedDate IS NULL THEN NULL ELSE datetime($SignedDate) END
			";

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					contract.Id,
					contract.ClientId,
					contract.ForwarderId,
					contract.RouteId,
					contract.ContractNumber,
					ContractDate = contract.ContractDate,
					contract.Terms,
					ContractStatus = (int)contract.ContractStatus,
					contract.PenaltyRatePerHour,
					contract.MaxPenaltyPercent,
					ValidUntil = contract.ValidUntil,
					SignedDate = contract.SignedDate
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}
	}
}
