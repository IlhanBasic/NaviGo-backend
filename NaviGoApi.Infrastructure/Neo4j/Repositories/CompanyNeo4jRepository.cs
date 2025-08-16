using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq;
using NaviGoApi.Common.DTOs;

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
			var session = _driver.AsyncSession(o => o.WithDefaultAccessMode(AccessMode.Write));

			try
			{
				var result = await session.WriteTransactionAsync(async tx =>
				{
					var query = @"
                MERGE (c:Counter { name: $entityName })
                ON CREATE SET c.lastId = 1
                ON MATCH SET c.lastId = c.lastId + 1
                RETURN c.lastId as lastId
            ";
					var res = await tx.RunAsync(query, new { entityName });
					var record = await res.SingleAsync();
					return record["lastId"].As<int>();
				});

				return result;
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
			var query = @"
        MATCH (c:Company {Id: $Id})
        OPTIONAL MATCH (c)-[:HAS_DRIVER]->(d:Driver)
        RETURN c, collect(d) AS drivers
        LIMIT 1
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });

				if (!await result.FetchAsync())
					return null;

				var cNode = result.Current["c"].As<INode>();
				var driversList = result.Current["drivers"].As<List<INode>>();

				var company = MapCompanyNode(cNode);

				// Mapiranje vozača u navigaciono svojstvo
				company.Drivers = driversList.Select(MapDriverNode).ToList();

				return company;
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
					company.CreatedAt,
					company.ProofFileUrl
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}
		private Driver MapDriverNode(INode node)
		{
			return new Driver
			{
				Id = node.Properties["Id"].As<int>(),
				CompanyId = node.Properties.ContainsKey("CompanyId") ? node.Properties["CompanyId"].As<int>() : 0,
				FirstName = node.Properties.GetValueOrDefault("FirstName")?.As<string>() ?? string.Empty,
				LastName = node.Properties.GetValueOrDefault("LastName")?.As<string>() ?? string.Empty,
				PhoneNumber = node.Properties.GetValueOrDefault("PhoneNumber")?.As<string>() ?? string.Empty,
				LicenseNumber = node.Properties.GetValueOrDefault("LicenseNumber")?.As<string>() ?? string.Empty,
				LicenseExpiry = node.Properties.ContainsKey("LicenseExpiry") && node.Properties["LicenseExpiry"] != null
					? ((ZonedDateTime)node.Properties["LicenseExpiry"]).ToDateTimeOffset().DateTime
					: (DateTime?)null,
				LicenseCategories = node.Properties.GetValueOrDefault("LicenseCategories")?.As<string>() ?? string.Empty,
				HireDate = node.Properties.ContainsKey("HireDate")
					? ((ZonedDateTime)node.Properties["HireDate"]).ToDateTimeOffset().DateTime
					: DateTime.MinValue,
				DriverStatus = node.Properties.ContainsKey("DriverStatus") ? (DriverStatus)(int)node.Properties["DriverStatus"].As<long>() : DriverStatus.Available,
				Shipments = new List<Shipment>()
			};
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
				CreatedAt = node["CreatedAt"].As<ZonedDateTime>().ToDateTimeOffset().UtcDateTime,
				ProofFileUrl = node.Properties.ContainsKey("ProofFileUrl") ? node["ProofFileUrl"].As<string?>() : null
			};
		}

		public Task<IEnumerable<Company>> GetAllAsync(CompanySearchDto companySearch)
		{
			throw new NotImplementedException();
		}
	}
}
