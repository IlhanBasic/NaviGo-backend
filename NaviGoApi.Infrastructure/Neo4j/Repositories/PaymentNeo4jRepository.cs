using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class PaymentNeo4jRepository : IPaymentRepository
	{
		private readonly IDriver _driver;

		public PaymentNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Payment payment)
		{
			var id = await GetNextIdAsync("Payment");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (p:Payment {
                    Id: $Id,
                    ContractId: $ContractId,
                    Amount: $Amount,
                    PaymentDate: datetime($PaymentDate),
                    PaymentStatus: $PaymentStatus,
                    ReceiptUrl: $ReceiptUrl,
                    ClientId: $ClientId
                })",
					new
					{
						Id = id,
						payment.ContractId,
						payment.Amount,
						PaymentDate = payment.PaymentDate.ToString("o"), // ISO 8601
						PaymentStatus = (int)payment.PaymentStatus,
						payment.ReceiptUrl,
						payment.ClientId
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Payment payment)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (p:Payment {Id: $Id})
				DETACH DELETE p",
				new { payment.Id });
		}

		public async Task<IEnumerable<Payment>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (p:Payment) RETURN p");
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["p"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (p:Payment {ClientId: $ClientId})
				RETURN p",
				new { ClientId = clientId });
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["p"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (p:Payment {ContractId: $ContractId})
				RETURN p",
				new { ContractId = contractId });
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["p"].As<INode>())).ToList();
		}

		public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (p:Payment)
				WHERE p.PaymentStatus = $PendingStatus
				RETURN p",
				new { PendingStatus = (int)PaymentStatus.Pending });
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["p"].As<INode>())).ToList();
		}

		public async Task<Payment?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (p:Payment {Id: $Id})
				RETURN p",
				new { Id = id });
			var records = await cursor.ToListAsync();
			if (records.Count == 0)
				return null;
			return MapNodeToEntity(records[0]["p"].As<INode>());
		}

		public async Task UpdateAsync(Payment payment)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (p:Payment {Id: $Id})
				SET p.ContractId = $ContractId,
					p.Amount = $Amount,
					p.PaymentDate = datetime($PaymentDate),
					p.PaymentStatus = $PaymentStatus,
					p.ReceiptUrl = $ReceiptUrl,
					p.ClientId = $ClientId",
				new
				{
					payment.Id,
					payment.ContractId,
					payment.Amount,
					PaymentDate = payment.PaymentDate.ToString("o"),
					PaymentStatus = (int)payment.PaymentStatus,
					payment.ReceiptUrl,
					payment.ClientId
				});
		}

		private Payment MapNodeToEntity(INode node)
		{
			return new Payment
			{
				Id = node.Properties["Id"].As<int>(),
				ContractId = node.Properties["ContractId"].As<int>(),
				Amount = node.Properties["Amount"].As<decimal>(),
				PaymentDate = DateTime.Parse(node.Properties["PaymentDate"].As<string>()),
				PaymentStatus = (PaymentStatus)Convert.ToInt32(node.Properties["PaymentStatus"]),
				ReceiptUrl = node.Properties.ContainsKey("ReceiptUrl") ? node.Properties["ReceiptUrl"].As<string>() : null,
				ClientId = node.Properties["ClientId"].As<int>()
			};
		}
	}
}
