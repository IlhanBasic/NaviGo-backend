using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ContractNeo4jRepository
	{
		private readonly IDriver _driver;

		public ContractNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Contract contract)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (contract:Contract {
                    Id: $id,
                    ClientId: $clientId,
                    ForwarderId: $forwarderId,
                    RouteId: $routeId,
                    ContractNumber: $contractNumber,
                    ContractDate: $contractDate,
                    Terms: $terms,
                    ContractStatus: $contractStatus,
                    PenaltyRatePerHour: $penaltyRatePerHour,
                    MaxPenaltyPercent: $maxPenaltyPercent,
                    ValidUntil: $validUntil,
                    SignedDate: $signedDate
                })
                WITH contract
                // Poveži sa User čvorom (Client)
                MATCH (client:User {Id: $clientId})
                CREATE (contract)-[:HAS_CLIENT]->(client)
                WITH contract
                // Poveži sa Company čvorom (Forwarder)
                MATCH (forwarder:Company {Id: $forwarderId})
                CREATE (contract)-[:HAS_FORWARDER]->(forwarder)
                WITH contract
                // Poveži sa Route čvorom
                MATCH (route:Route {Id: $routeId})
                CREATE (contract)-[:HAS_ROUTE]->(route)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = contract.Id,
				["clientId"] = contract.ClientId,
				["forwarderId"] = contract.ForwarderId,
				["routeId"] = contract.RouteId,
				["contractNumber"] = contract.ContractNumber,
				["contractDate"] = contract.ContractDate.ToString("o"),
				["terms"] = contract.Terms,
				["contractStatus"] = (int)contract.ContractStatus,
				["penaltyRatePerHour"] = contract.PenaltyRatePerHour,
				["maxPenaltyPercent"] = contract.MaxPenaltyPercent,
				["validUntil"] = contract.ValidUntil.ToString("o"),
				["signedDate"] = contract.SignedDate?.ToString("o")
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (contract:Contract)
                OPTIONAL MATCH (contract)-[:HAS_CLIENT]->(client:User)
                OPTIONAL MATCH (contract)-[:HAS_FORWARDER]->(forwarder:Company)
                OPTIONAL MATCH (contract)-[:HAS_ROUTE]->(route:Route)
                RETURN contract, client, forwarder, route
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var contracts = new List<Contract>();

				foreach (var record in records)
				{
					var contractNode = record["contract"].As<INode>();
					var clientNode = record["client"]?.As<INode>();
					var forwarderNode = record["forwarder"]?.As<INode>();
					var routeNode = record["route"]?.As<INode>();

					var contract = MapNodeToContract(contractNode);

					if (clientNode != null)
						contract.Client = new User { Id = (int)(long)clientNode.Properties["Id"] }; // možeš dodati mapiranje svih polja

					if (forwarderNode != null)
						contract.Forwarder = new Company { Id = (int)(long)forwarderNode.Properties["Id"] };

					if (routeNode != null)
						contract.Route = new Route { Id = (int)(long)routeNode.Properties["Id"] };

					contracts.Add(contract);
				}

				return contracts;
			});
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (contract:Contract {Id: $id})
                OPTIONAL MATCH (contract)-[:HAS_CLIENT]->(client:User)
                OPTIONAL MATCH (contract)-[:HAS_FORWARDER]->(forwarder:Company)
                OPTIONAL MATCH (contract)-[:HAS_ROUTE]->(route:Route)
                RETURN contract, client, forwarder, route
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var contractNode = record["contract"].As<INode>();
				var clientNode = record["client"]?.As<INode>();
				var forwarderNode = record["forwarder"]?.As<INode>();
				var routeNode = record["route"]?.As<INode>();

				var contract = MapNodeToContract(contractNode);

				if (clientNode != null)
					contract.Client = new User { Id = (int)(long)clientNode.Properties["Id"] };

				if (forwarderNode != null)
					contract.Forwarder = new Company { Id = (int)(long)forwarderNode.Properties["Id"] };

				if (routeNode != null)
					contract.Route = new Route { Id = (int)(long)routeNode.Properties["Id"] };

				return contract;
			});
		}

		public async Task UpdateAsync(Contract contract)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (contract:Contract {Id: $id})
                SET contract.ContractNumber = $contractNumber,
                    contract.ContractDate = $contractDate,
                    contract.Terms = $terms,
                    contract.ContractStatus = $contractStatus,
                    contract.PenaltyRatePerHour = $penaltyRatePerHour,
                    contract.MaxPenaltyPercent = $maxPenaltyPercent,
                    contract.ValidUntil = $validUntil,
                    contract.SignedDate = $signedDate
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = contract.Id,
				["contractNumber"] = contract.ContractNumber,
				["contractDate"] = contract.ContractDate.ToString("o"),
				["terms"] = contract.Terms,
				["contractStatus"] = (int)contract.ContractStatus,
				["penaltyRatePerHour"] = contract.PenaltyRatePerHour,
				["maxPenaltyPercent"] = contract.MaxPenaltyPercent,
				["validUntil"] = contract.ValidUntil.ToString("o"),
				["signedDate"] = contract.SignedDate?.ToString("o")
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Contract {Id: $id}) DETACH DELETE c";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		private Contract MapNodeToContract(INode node)
		{
			return new Contract
			{
				Id = (int)(long)node.Properties["Id"],
				ClientId = (int)(long)node.Properties["ClientId"],
				ForwarderId = (int)(long)node.Properties["ForwarderId"],
				RouteId = (int)(long)node.Properties["RouteId"],
				ContractNumber = (string)node.Properties["ContractNumber"],
				ContractDate = DateTime.Parse((string)node.Properties["ContractDate"]),
				Terms = (string)node.Properties["Terms"],
				ContractStatus = (ContractStatus)(int)(long)node.Properties["ContractStatus"],
				PenaltyRatePerHour = Convert.ToDecimal((double)node.Properties["PenaltyRatePerHour"]),
				MaxPenaltyPercent = Convert.ToDecimal((double)node.Properties["MaxPenaltyPercent"]),
				ValidUntil = DateTime.Parse((string)node.Properties["ValidUntil"]),
				SignedDate = node.Properties.ContainsKey("SignedDate") && node.Properties["SignedDate"] != null
					? DateTime.Parse((string)node.Properties["SignedDate"])
					: (DateTime?)null
			};
		}
	}
}
