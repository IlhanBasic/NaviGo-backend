using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
			contract.Id = id;
			var query = @"
                CREATE (c:Contract {
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
                    SignedDate: $signedDate
                })";

			var session = _driver.AsyncSession();
			await session.RunAsync(query, new
			{
				id = id,
				clientId = contract.ClientId,
				forwarderId = contract.ForwarderId,
				routeId = contract.RouteId,
				contractNumber = contract.ContractNumber,
				contractDate = contract.ContractDate,
				terms = contract.Terms,
				contractStatus = (int)contract.ContractStatus,
				penaltyRatePerHour = contract.PenaltyRatePerHour,
				maxPenaltyPercent = contract.MaxPenaltyPercent,
				signedDate = contract.SignedDate
			});
			await session.CloseAsync();
		}

		public async Task DeleteAsync(Contract contract)
		{
			var query = @"MATCH (c:Contract {Id: $id}) DETACH DELETE c";
			var session = _driver.AsyncSession();
			await session.RunAsync(query, new { id = contract.Id });
			await session.CloseAsync();
		}

		public async Task<bool> DuplicateContract(string contractNumber)
		{
			// 1. Pronađi postojeći ugovor po contractNumber
			var queryFind = @"MATCH (c:Contract { ContractNumber: $contractNumber }) RETURN c LIMIT 1";
			var session = _driver.AsyncSession();
			Contract? existing = null;

			try
			{
				var result = await session.RunAsync(queryFind, new { contractNumber });
				if (await result.FetchAsync())
				{
					var node = result.Current["c"].As<INode>();
					existing = MapNodeToContract(node);
				}
			}
			finally
			{
				await session.CloseAsync();
			}

			if (existing == null)
				return false; // ugovor sa tim brojem ne postoji

			// 2. Generiši novi Id
			int newId;
			session = _driver.AsyncSession();
			try
			{
				var counterQuery = @"
            MERGE (c:Counter { name: 'Contract' })
            ON CREATE SET c.lastId = 1
            ON MATCH SET c.lastId = c.lastId + 1
            RETURN c.lastId AS lastId
        ";
				var counterResult = await session.RunAsync(counterQuery);
				var record = await counterResult.SingleAsync();
				newId = record["lastId"].As<int>();
			}
			finally
			{
				await session.CloseAsync();
			}

			// 3. Kreiraj kopiju ugovora sa novim Id-jem
			var queryCreate = @"
        CREATE (c:Contract {
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
            SignedDate: $signedDate
        })
    ";

			session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(queryCreate, new
				{
					id = newId,
					clientId = existing.ClientId,
					forwarderId = existing.ForwarderId,
					routeId = existing.RouteId,
					contractNumber = existing.ContractNumber,
					contractDate = existing.ContractDate,
					terms = existing.Terms,
					contractStatus = (int)existing.ContractStatus,
					penaltyRatePerHour = existing.PenaltyRatePerHour,
					maxPenaltyPercent = existing.MaxPenaltyPercent,
					signedDate = existing.SignedDate
				});
			}
			finally
			{
				await session.CloseAsync();
			}

			return true;
		}

		public async Task<IEnumerable<Contract>> GetAllAsync()
		{
			var query = @"MATCH (c:Contract) RETURN c";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query);

			var list = new List<Contract>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToContract(result.Current["c"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<IEnumerable<Contract>> GetByClientIdAsync(int clientId)
		{
			var query = @"MATCH (c:Contract {ClientId: $clientId}) RETURN c";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { clientId });

			var list = new List<Contract>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToContract(result.Current["c"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<IEnumerable<Contract>> GetByForwarderIdAsync(int forwarderId)
		{
			var query = @"MATCH (c:Contract {ForwarderId: $forwarderId}) RETURN c";
			var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { forwarderId });

			var list = new List<Contract>();
			while (await result.FetchAsync())
			{
				list.Add(MapNodeToContract(result.Current["c"].As<INode>()));
			}
			await session.CloseAsync();
			return list;
		}

		public async Task<Contract?> GetByIdAsync(int id)
		{
			var query = @"
        MATCH (c:Contract {Id: $id})
        OPTIONAL MATCH (c)-[:HAS_ROUTE]->(r:Route)
        RETURN c, r
        LIMIT 1
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { id });

				if (!await result.FetchAsync())
					return null;

				var cNode = result.Current["c"].As<INode>();
				var rNode = result.Current["r"]?.As<INode>();

				var contract = MapNodeToContract(cNode);

				if (rNode != null)
					contract.Route = MapNodeToRoute(rNode);

				return contract;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Contract contract)
		{
			var query = @"
        MATCH (c:Contract {Id: $id})
        SET c.ClientId = $clientId,
            c.ForwarderId = $forwarderId,
            c.RouteId = $routeId,
            c.ContractNumber = $contractNumber,
            c.ContractDate = $contractDate,
            c.Terms = $terms,
            c.ContractStatus = $contractStatus,
            c.PenaltyRatePerHour = $penaltyRatePerHour,
            c.MaxPenaltyPercent = $maxPenaltyPercent,
            c.SignedDate = $signedDate
    ";

			// Koristi WriteTransaction za commit
			var session = _driver.AsyncSession();
			try
			{
				await session.WriteTransactionAsync(async tx =>
				{
					await tx.RunAsync(query, new
					{
						id = contract.Id.ToString(),
						clientId = contract.ClientId,
						forwarderId = contract.ForwarderId,
						routeId = contract.RouteId,
						contractNumber = contract.ContractNumber,
						contractDate = contract.ContractDate,
						terms = contract.Terms,
						contractStatus = (int)contract.ContractStatus,
						penaltyRatePerHour = contract.PenaltyRatePerHour,
						maxPenaltyPercent = contract.MaxPenaltyPercent,
						signedDate = contract.SignedDate
					});
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private Route MapNodeToRoute(INode node)
		{
			return new Route
			{
				Id = node.Properties["Id"].As<int>(),
				CompanyId = node.Properties["CompanyId"].As<int>(),
				StartLocationId = node.Properties["StartLocationId"].As<int>(),
				EndLocationId = node.Properties["EndLocationId"].As<int>(),
				DistanceKm = node.Properties.ContainsKey("DistanceKm") ? Convert.ToDouble(node.Properties["DistanceKm"]) : 0,
				EstimatedDurationHours = node.Properties.ContainsKey("EstimatedDurationHours") ? Convert.ToDouble(node.Properties["EstimatedDurationHours"]) : 0,
				IsActive = node.Properties.ContainsKey("IsActive") ? Convert.ToBoolean(node.Properties["IsActive"]) : true,
				CreatedAt = node.Properties.ContainsKey("CreatedAt") ? Convert.ToDateTime(node.Properties["CreatedAt"]) : DateTime.UtcNow,
				AvailableFrom = node.Properties.ContainsKey("AvailableFrom") ? Convert.ToDateTime(node.Properties["AvailableFrom"]) : DateTime.UtcNow,
				AvailableTo = node.Properties.ContainsKey("AvailableTo") ? Convert.ToDateTime(node.Properties["AvailableTo"]) : DateTime.UtcNow,
				GeometryEncoded = node.Properties.ContainsKey("GeometryEncoded") ? node.Properties["GeometryEncoded"].ToString() : null
			};
		}

		private Contract MapNodeToContract(INode node)
		{
			DateTime ConvertNeo4jDateTime(string key)
			{
				if (!node.Properties.ContainsKey(key) || node.Properties[key] == null)
					return default;

				var zoned = node.Properties[key].As<ZonedDateTime>();
				return zoned.ToDateTimeOffset().UtcDateTime;
			}

			DateTime? ConvertNeo4jDateTimeNullable(string key)
			{
				if (!node.Properties.ContainsKey(key) || node.Properties[key] == null)
					return null;

				var zoned = node.Properties[key].As<ZonedDateTime>();
				return zoned.ToDateTimeOffset().UtcDateTime;
			}

			return new Contract
			{
				Id = node.Properties["Id"].As<int>(),
				ClientId = node.Properties["ClientId"].As<int>(),
				ForwarderId = node.Properties["ForwarderId"].As<int>(),
				RouteId = node.Properties["RouteId"].As<int>(),
				ContractNumber = node.Properties["ContractNumber"].As<string>(),
				ContractDate = ConvertNeo4jDateTime("ContractDate"),
				Terms = node.Properties["Terms"].As<string>(),
				ContractStatus = (ContractStatus)(int)node.Properties["ContractStatus"].As<long>(),
				PenaltyRatePerHour = node.Properties["PenaltyRatePerHour"].As<decimal>(),
				MaxPenaltyPercent = node.Properties["MaxPenaltyPercent"].As<decimal>(),
				SignedDate = ConvertNeo4jDateTimeNullable("SignedDate")
			};
		}

		public async Task<IEnumerable<Contract>> GetAllAsync(ContractSearchDto contractSearch)
		{
			var query = @"
        MATCH (c:Contract)
        WHERE $contractNumber IS NULL OR c.ContractNumber CONTAINS $contractNumber
        RETURN c
        ORDER BY CASE $sortBy 
                    WHEN 'ContractNumber' THEN c.ContractNumber
                    WHEN 'ContractDate' THEN c.ContractDate
                    ELSE c.Id
                 END " + (contractSearch.SortDirection.ToLower() == "desc" ? "DESC" : "ASC") + @"
        SKIP $skip
        LIMIT $limit";

			var skip = (contractSearch.Page - 1) * contractSearch.PageSize;
			var limit = contractSearch.PageSize;

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new
				{
					contractNumber = string.IsNullOrWhiteSpace(contractSearch.ContractNumber) ? null : contractSearch.ContractNumber,
					sortBy = contractSearch.SortBy,
					skip,
					limit
				});

				var list = new List<Contract>();
				while (await result.FetchAsync())
				{
					list.Add(MapNodeToContract(result.Current["c"].As<INode>()));
				}

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

	}
}
