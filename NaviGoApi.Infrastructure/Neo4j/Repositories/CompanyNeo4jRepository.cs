using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class CompanyNeo4jRepository : ICompanyRepository
	{
		private readonly IDriver _driver;

		public CompanyNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Company company)
		{
			var id = await GetNextIdAsync("Company");

			var query = @"
            CREATE (c:Company {
                Id: $Id,
                CompanyName: $CompanyName,
                PIB: $PIB,
                Address: $Address,
                LogoUrl: $LogoUrl,
                ContactEmail: $ContactEmail,
                Website: $Website,
                Description: $Description,
                CompanyType: $CompanyType,
                CompanyStatus: $CompanyStatus,
                MaxCommissionRate: $MaxCommissionRate,
                SaldoAmount: $SaldoAmount,
                CreatedAt: $CreatedAt,
                ProofFileUrl: $ProofFileUrl
            })
        ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					company.CompanyName,
					company.PIB,
					company.Address,
					company.LogoUrl,
					company.ContactEmail,
					company.Website,
					company.Description,
					CompanyType = (int)company.CompanyType,
					CompanyStatus = (int)company.CompanyStatus,
					company.MaxCommissionRate,
					company.SaldoAmount,
					company.CreatedAt,
					company.ProofFileUrl
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(Company company)
		{
			var query = @"MATCH (c:Company { Id: $Id }) DETACH DELETE c";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { company.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Company>> GetAllAsync()
		{
			var query = @"MATCH (c:Company) RETURN c";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var list = new List<Company>();

				await result.ForEachAsync(record =>
				{
					var node = record["c"].As<INode>();
					list.Add(MapCompanyNode(node));
				});

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Company?> GetByIdAsync(int id)
		{
			var query = @"MATCH (c:Company { Id: $Id }) RETURN c";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var records = await result.ToListAsync();
				var record = records.FirstOrDefault();

				if (record == null) return null;

				return MapCompanyNode(record["c"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Company?> GetByNameAsync(string name)
		{
			var query = @"MATCH (c:Company { CompanyName: $CompanyName }) RETURN c";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { CompanyName = name });
				var records = await result.ToListAsync();
				var record = records.FirstOrDefault();

				if (record == null) return null;

				return MapCompanyNode(record["c"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Company?> GetByPibAsync(string pib)
		{
			var query = @"MATCH (c:Company { PIB: $PIB }) RETURN c";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { PIB = pib });
				var records = await result.ToListAsync();
				var record = records.FirstOrDefault();

				if (record == null) return null;

				return MapCompanyNode(record["c"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Company company)
		{
			var query = @"
                MATCH (c:Company { Id: $Id })
                SET c.CompanyName = $CompanyName,
                    c.PIB = $PIB,
                    c.Address = $Address,
                    c.LogoUrl = $LogoUrl,
                    c.ContactEmail = $ContactEmail,
                    c.Website = $Website,
                    c.Description = $Description,
                    c.CompanyType = $CompanyType,
                    c.CompanyStatus = $CompanyStatus,
                    c.MaxCommissionRate = $MaxCommissionRate,
                    c.SaldoAmount = $SaldoAmount,
                    c.CreatedAt = $CreatedAt,
                    c.ProofFileUrl = $ProofFileUrl
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					company.Id,
					company.CompanyName,
					company.PIB,
					company.Address,
					company.LogoUrl,
					company.ContactEmail,
					company.Website,
					company.Description,
					CompanyType = (int)company.CompanyType,
					CompanyStatus = (int)company.CompanyStatus,
					company.MaxCommissionRate,
					company.SaldoAmount,
					company.CreatedAt,
					company.ProofFileUrl
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private Company MapCompanyNode(INode node)
		{
			return new Company
			{
				Id = node["Id"].As<int>(),
				CompanyName = node["CompanyName"].As<string>(),
				PIB = node["PIB"].As<string>(),
				Address = node["Address"].As<string>(),
				LogoUrl = node.Properties.ContainsKey("LogoUrl") ? node["LogoUrl"].As<string?>() : null,
				ContactEmail = node["ContactEmail"].As<string>(),
				Website = node.Properties.ContainsKey("Website") ? node["Website"].As<string?>() : null,
				Description = node.Properties.ContainsKey("Description") ? node["Description"].As<string?>() : null,
				CompanyType = (CompanyType)node["CompanyType"].As<int>(),
				CompanyStatus = (CompanyStatus)node["CompanyStatus"].As<int>(),
				MaxCommissionRate = node.Properties.ContainsKey("MaxCommissionRate") ? node["MaxCommissionRate"].As<decimal?>() : null,
				SaldoAmount = node.Properties.ContainsKey("SaldoAmount") ? node["SaldoAmount"].As<decimal?>() : null,
				CreatedAt = node["CreatedAt"].As<DateTime>(),
				ProofFileUrl = node.Properties.ContainsKey("ProofFileUrl") ? node["ProofFileUrl"].As<string?>() : null
			};
		}
	}
}
