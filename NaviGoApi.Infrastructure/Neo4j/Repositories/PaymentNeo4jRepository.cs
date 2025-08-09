using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class PaymentNeo4jRepository
	{
		private readonly IDriver _driver;

		public PaymentNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Payment payment)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (p:Payment {
                    Id: $id,
                    ClientId: $clientId,
                    ContractId: $contractId,
                    Amount: $amount,
                    PaymentDate: $paymentDate,
                    PaymentStatus: $paymentStatus
                })
                WITH p
                MATCH (c:User {Id: $clientId}), (con:Contract {Id: $contractId})
                CREATE (p)-[:BY_CLIENT]->(c)
                CREATE (p)-[:FOR_CONTRACT]->(con)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = payment.Id,
				["clientId"] = payment.ClientId,
				["contractId"] = payment.ContractId,
				["amount"] = payment.Amount,
				["paymentDate"] = payment.PaymentDate.ToString("o"),
				["paymentStatus"] = (int)payment.PaymentStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (p:Payment {Id: $id}) DETACH DELETE p";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<Payment>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment)-[:BY_CLIENT]->(c:User),
                      (p)-[:FOR_CONTRACT]->(con:Contract)
                RETURN p, c, con
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var payments = new List<Payment>();

				foreach (var record in records)
				{
					var nodePayment = record["p"].As<INode>();
					var nodeClient = record["c"].As<INode>();
					var nodeContract = record["con"].As<INode>();

					var payment = MapNodeToPayment(nodePayment);
					payment.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };
					payment.Contract = new Contract { Id = (int)(long)nodeContract.Properties["Id"] };

					payments.Add(payment);
				}

				return payments;
			});
		}

		public async Task<Payment?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment {Id: $id})-[:BY_CLIENT]->(c:User),
                      (p)-[:FOR_CONTRACT]->(con:Contract)
                RETURN p, c, con
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();

				if (!hasRecord) return null;

				var record = cursor.Current;
				var nodePayment = record["p"].As<INode>();
				var nodeClient = record["c"].As<INode>();
				var nodeContract = record["con"].As<INode>();

				var payment = MapNodeToPayment(nodePayment);
				payment.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };
				payment.Contract = new Contract { Id = (int)(long)nodeContract.Properties["Id"] };

				return payment;
			});
		}

		public async Task<IEnumerable<Payment>> GetByClientIdAsync(int clientId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment {ClientId: $clientId})-[:BY_CLIENT]->(c:User),
                      (p)-[:FOR_CONTRACT]->(con:Contract)
                RETURN p, c, con
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { clientId });
				var records = await cursor.ToListAsync();

				var payments = new List<Payment>();

				foreach (var record in records)
				{
					var nodePayment = record["p"].As<INode>();
					var nodeClient = record["c"].As<INode>();
					var nodeContract = record["con"].As<INode>();

					var payment = MapNodeToPayment(nodePayment);
					payment.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };
					payment.Contract = new Contract { Id = (int)(long)nodeContract.Properties["Id"] };

					payments.Add(payment);
				}

				return payments;
			});
		}

		public async Task<IEnumerable<Payment>> GetByContractIdAsync(int contractId)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment {ContractId: $contractId})-[:BY_CLIENT]->(c:User),
                      (p)-[:FOR_CONTRACT]->(con:Contract)
                RETURN p, c, con
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { contractId });
				var records = await cursor.ToListAsync();

				var payments = new List<Payment>();

				foreach (var record in records)
				{
					var nodePayment = record["p"].As<INode>();
					var nodeClient = record["c"].As<INode>();
					var nodeContract = record["con"].As<INode>();

					var payment = MapNodeToPayment(nodePayment);
					payment.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };
					payment.Contract = new Contract { Id = (int)(long)nodeContract.Properties["Id"] };

					payments.Add(payment);
				}

				return payments;
			});
		}

		public async Task<IEnumerable<Payment>> GetPendingPaymentsAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment)
                WHERE p.PaymentStatus = $pendingStatus
                OPTIONAL MATCH (p)-[:BY_CLIENT]->(c:User)
                OPTIONAL MATCH (p)-[:FOR_CONTRACT]->(con:Contract)
                RETURN p, c, con
            ";

			var pendingStatus = (int)PaymentStatus.Pending;

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { pendingStatus });
				var records = await cursor.ToListAsync();

				var payments = new List<Payment>();

				foreach (var record in records)
				{
					var nodePayment = record["p"].As<INode>();

					var payment = MapNodeToPayment(nodePayment);

					if (record["c"] is INode nodeClient)
						payment.Client = new User { Id = (int)(long)nodeClient.Properties["Id"] };

					if (record["con"] is INode nodeContract)
						payment.Contract = new Contract { Id = (int)(long)nodeContract.Properties["Id"] };

					payments.Add(payment);
				}

				return payments;
			});
		}

		public async Task UpdateAsync(Payment payment)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (p:Payment {Id: $id})
                SET p.ClientId = $clientId,
                    p.ContractId = $contractId,
                    p.Amount = $amount,
                    p.PaymentDate = $paymentDate,
                    p.PaymentStatus = $paymentStatus
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = payment.Id,
				["clientId"] = payment.ClientId,
				["contractId"] = payment.ContractId,
				["amount"] = payment.Amount,
				["paymentDate"] = payment.PaymentDate.ToString("o"),
				["paymentStatus"] = (int)payment.PaymentStatus
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private Payment MapNodeToPayment(INode node)
		{
			return new Payment
			{
				Id = (int)(long)node.Properties["Id"],
				ClientId = (int)(long)node.Properties["ClientId"],
				ContractId = (int)(long)node.Properties["ContractId"],
				Amount = Convert.ToDecimal((double)node.Properties["Amount"]),
				PaymentDate = DateTime.Parse((string)node.Properties["PaymentDate"]),
				PaymentStatus = (PaymentStatus)(int)(long)node.Properties["PaymentStatus"]
			};
		}
	}
}
