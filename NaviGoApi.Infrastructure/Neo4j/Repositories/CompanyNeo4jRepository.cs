using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class CompanyNeo4jRepository
	{
		private readonly IDriver _driver;

		public CompanyNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Company company)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (c:Company {
                    Id: $id,
                    CompanyName: $companyName,
                    PIB: $pib,
                    Address: $address,
                    LogoUrl: $logoUrl,
                    ContactEmail: $contactEmail,
                    Website: $website,
                    Description: $description,
                    CompanyType: $companyType,
                    CompanyStatus: $companyStatus,
                    MaxCommissionRate: $maxCommissionRate,
                    SaldoAmount: $saldoAmount,
                    CreatedAt: $createdAt,
                    ProofFileUrl: $proofFileUrl
                })";

			var parameters = new Dictionary<string, object>
			{
				["id"] = company.Id,
				["companyName"] = company.CompanyName,
				["pib"] = company.PIB,
				["address"] = company.Address,
				["logoUrl"] = company.LogoUrl ?? "",
				["contactEmail"] = company.ContactEmail,
				["website"] = company.Website ?? "",
				["description"] = company.Description ?? "",
				["companyType"] = (int)company.CompanyType,
				["companyStatus"] = (int)company.CompanyStatus,
				["maxCommissionRate"] = company.MaxCommissionRate ?? 0m,
				["saldoAmount"] = company.SaldoAmount ?? 0m,
				["createdAt"] = company.CreatedAt.ToString("o"), // ISO 8601
				["proofFileUrl"] = company.ProofFileUrl ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task<IEnumerable<Company>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Company) RETURN c";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();
				var companies = new List<Company>();

				foreach (var record in records)
				{
					var node = record["c"].As<INode>();
					companies.Add(MapNodeToCompany(node));
				}

				return companies;
			});
		}

		public async Task<Company?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Company {Id: $id}) RETURN c LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["c"].As<INode>();

				return MapNodeToCompany(node);
			});
		}

		public async Task<Company?> GetByNameAsync(string name)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Company {CompanyName: $name}) RETURN c LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { name });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["c"].As<INode>();

				return MapNodeToCompany(node);
			});
		}

		public async Task<Company?> GetByPibAsync(string pib)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Company {PIB: $pib}) RETURN c LIMIT 1";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { pib });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["c"].As<INode>();

				return MapNodeToCompany(node);
			});
		}

		public async Task UpdateAsync(Company company)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (c:Company {Id: $id})
                SET c.CompanyName = $companyName,
                    c.PIB = $pib,
                    c.Address = $address,
                    c.LogoUrl = $logoUrl,
                    c.ContactEmail = $contactEmail,
                    c.Website = $website,
                    c.Description = $description,
                    c.CompanyType = $companyType,
                    c.CompanyStatus = $companyStatus,
                    c.MaxCommissionRate = $maxCommissionRate,
                    c.SaldoAmount = $saldoAmount,
                    c.CreatedAt = $createdAt,
                    c.ProofFileUrl = $proofFileUrl";

			var parameters = new Dictionary<string, object>
			{
				["id"] = company.Id,
				["companyName"] = company.CompanyName,
				["pib"] = company.PIB,
				["address"] = company.Address,
				["logoUrl"] = company.LogoUrl ?? "",
				["contactEmail"] = company.ContactEmail,
				["website"] = company.Website ?? "",
				["description"] = company.Description ?? "",
				["companyType"] = (int)company.CompanyType,
				["companyStatus"] = (int)company.CompanyStatus,
				["maxCommissionRate"] = company.MaxCommissionRate ?? 0m,
				["saldoAmount"] = company.SaldoAmount ?? 0m,
				["createdAt"] = company.CreatedAt.ToString("o"),
				["proofFileUrl"] = company.ProofFileUrl ?? ""
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:Company {Id: $id}) DETACH DELETE c";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		// Helper to map Neo4j node to Company entity
		private Company MapNodeToCompany(INode node)
		{
			return new Company
			{
				Id = (int)(long)node.Properties["Id"],
				CompanyName = (string)node.Properties["CompanyName"],
				PIB = (string)node.Properties["PIB"],
				Address = (string)node.Properties["Address"],
				LogoUrl = node.Properties.ContainsKey("LogoUrl") ? (string)node.Properties["LogoUrl"] : null,
				ContactEmail = (string)node.Properties["ContactEmail"],
				Website = node.Properties.ContainsKey("Website") ? (string)node.Properties["Website"] : null,
				Description = node.Properties.ContainsKey("Description") ? (string)node.Properties["Description"] : null,
				CompanyType = (CompanyType)(int)(long)node.Properties["CompanyType"],
				CompanyStatus = (CompanyStatus)(int)(long)node.Properties["CompanyStatus"],
				MaxCommissionRate = node.Properties.ContainsKey("MaxCommissionRate") ? (decimal)(double)node.Properties["MaxCommissionRate"] : null,
				SaldoAmount = node.Properties.ContainsKey("SaldoAmount") ? (decimal)(double)node.Properties["SaldoAmount"] : null,
				CreatedAt = node.Properties.ContainsKey("CreatedAt") ? DateTime.Parse((string)node.Properties["CreatedAt"]) : DateTime.MinValue,
				ProofFileUrl = node.Properties.ContainsKey("ProofFileUrl") ? (string)node.Properties["ProofFileUrl"] : null
			};
		}
	}
}
