using NaviGoApi.Common.DTOs;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class UserNeo4jRepository : IUserRepository
	{
		private readonly IDriver _driver;

		public UserNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(User user)
		{
			var id = await GetNextIdAsync("User");

			var query = @"
                CREATE (u:User {
                    Id: $Id,
                    Email: $Email,
                    PasswordHash: $PasswordHash,
                    FirstName: $FirstName,
                    LastName: $LastName,
                    PhoneNumber: $PhoneNumber,
                    UserRole: $UserRole,
                    CreatedAt: $CreatedAt,
                    LastLogin: $LastLogin,
                    EmailVerified: $EmailVerified,
                    UserStatus: $UserStatus,
                    CompanyId: $CompanyId,
                    EmailVerificationToken: $EmailVerificationToken,
                    PasswordResetToken: $PasswordResetToken
                })
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					Email = user.Email,
					PasswordHash = user.PasswordHash,
					FirstName = user.FirstName,
					LastName = user.LastName,
					PhoneNumber = user.PhoneNumber,
					UserRole = (int)user.UserRole,
					CreatedAt = user.CreatedAt,
					LastLogin = user.LastLogin,
					EmailVerified = user.EmailVerified,
					UserStatus = (int)user.UserStatus,
					CompanyId = user.CompanyId,
					EmailVerificationToken = user.EmailVerificationToken,
					PasswordResetToken = user.PasswordResetToken
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(User user)
		{
			var query = @"MATCH (u:User { Id: $Id }) DETACH DELETE u";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { Id = user.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			var query = @"
                MATCH (u:User)
                OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
                RETURN u, c
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var records = await result.ToListAsync();

				return records.Select(record =>
				{
					var uNode = record["u"].As<INode>();
					var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;

					return MapUserNode(uNode, cNode);
				}).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			var query = @"
                MATCH (u:User { Id: $Id })
                OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
                RETURN u, c
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				var uNode = record["u"].As<INode>();
				var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;

				return MapUserNode(uNode, cNode);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			var query = @"
        MATCH (u:User { Email: $Email })
        OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
        RETURN u, c
    ";

			await using var session = _driver.AsyncSession();
			var result = await session.RunAsync(query, new { Email = email });

			var record = (await result.ToListAsync()).FirstOrDefault();
			if (record == null)
				return null;

			var uNode = record["u"].As<INode>();
			INode? cNode = record["c"] is INode node ? node : null;

			return MapUserNode(uNode, cNode);
		}

		public async Task<User?> GetByPasswordResetTokenAsync(string token)
		{
			var query = @"
                MATCH (u:User { PasswordResetToken: $Token })
                OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
                RETURN u, c
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Token = token });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				var uNode = record["u"].As<INode>();
				var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;

				return MapUserNode(uNode, cNode);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByEmailVerificationTokenAsync(string token)
		{
			var query = @"
                MATCH (u:User { EmailVerificationToken: $Token })
                OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
                RETURN u, c
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Token = token });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				var uNode = record["u"].As<INode>();
				var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;

				return MapUserNode(uNode, cNode);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			var query = @"MATCH (rt:RefreshToken { Token: $Token }) RETURN rt";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Token = token });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				var rtNode = record["rt"].As<INode>();
				return MapRefreshTokenNode(rtNode);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task AddRefreshTokenAsync(RefreshToken token)
		{
			var id = await GetNextIdAsync("RefreshToken");

			var query = @"
        MATCH (u:User { Id: $UserId })
        CREATE (rt:RefreshToken {
            Id: $Id,
            Token: $Token,
            Expires: datetime($Expires),
            Created: datetime($Created),
            CreatedByIp: $CreatedByIp,
            UserId: $UserId
        })
        CREATE (u)-[:HAS_REFRESH_TOKEN]->(rt)
    ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = (long)id,
					Token = token.Token,
					Expires = token.Expires.ToUniversalTime().ToString("o"), // ISO 8601
					Created = token.Created.ToUniversalTime().ToString("o"), // ISO 8601
					CreatedByIp = token.CreatedByIp,
					UserId = (long)token.UserId
				});

				Console.WriteLine($"Added refresh token for UserId: {token.UserId}, Token: {token.Token}");
			}
			finally
			{
				await session.CloseAsync();
			}
		}


		public async Task UpdateRefreshTokenAsync(RefreshToken token)
		{
			var query = @"
                MATCH (rt:RefreshToken { Id: $Id })
                SET rt.Token = $Token,
                    rt.Expires = $Expires,
                    rt.Revoked = $Revoked,
                    rt.RevokedByIp = $RevokedByIp,
                    rt.ReplacedByToken = $ReplacedByToken
            ";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					token.Id,
					token.Token,
					token.Expires,
					token.Revoked,
					token.RevokedByIp,
					token.ReplacedByToken
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(User user)
		{
			var query = @"
                MATCH (u:User { Id: $Id })
                SET u.Email = $Email,
                    u.PasswordHash = $PasswordHash,
                    u.FirstName = $FirstName,
                    u.LastName = $LastName,
                    u.PhoneNumber = $PhoneNumber,
                    u.UserRole = $UserRole,
                    u.LastLogin = $LastLogin,
                    u.EmailVerified = $EmailVerified,
                    u.UserStatus = $UserStatus,
                    u.CompanyId = $CompanyId
            ";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					user.Id,
					user.Email,
					user.PasswordHash,
					user.FirstName,
					user.LastName,
					user.PhoneNumber,
					UserRole = (int)user.UserRole,
					user.LastLogin,
					user.EmailVerified,
					UserStatus = (int)user.UserStatus,
					user.CompanyId
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public IQueryable<User> Query()
		{
			throw new NotImplementedException("Neo4j does not support IQueryable.");
		}

		private async Task<int> GetNextIdAsync(string entityName)
		{
			var query = @"
                MERGE (c:Counter { name: $entityName })
                ON CREATE SET c.lastId = 1
                ON MATCH SET c.lastId = c.lastId + 1
                RETURN c.lastId AS lastId
            ";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { entityName });
				var record = (await result.ToListAsync()).First();
				return record["lastId"].As<int>();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private User MapUserNode(INode uNode, INode? cNode)
		{
			var user = new User
			{
				Id = uNode["Id"].As<int>(),
				Email = uNode["Email"].As<string>(),
				PasswordHash = uNode["PasswordHash"].As<string>(),
				FirstName = uNode["FirstName"].As<string>(),
				LastName = uNode["LastName"].As<string>(),
				PhoneNumber = uNode["PhoneNumber"].As<string>(),
				UserRole = (UserRole)uNode["UserRole"].As<int>(),
				CreatedAt = uNode["CreatedAt"].As<ZonedDateTime>().ToDateTimeOffset().LocalDateTime,
				LastLogin = uNode.Properties.ContainsKey("LastLogin") ? uNode["LastLogin"].As<DateTime?>() : null,
				EmailVerified = uNode["EmailVerified"].As<bool>(),
				UserStatus = (UserStatus)uNode["UserStatus"].As<int>(),
				CompanyId = uNode.Properties.ContainsKey("CompanyId") ? uNode["CompanyId"].As<int?>() : null
			};

			if (cNode != null)
			{
				user.Company = new Company
				{
					Id = cNode["Id"].As<int>(),
					CompanyName = cNode["CompanyName"].As<string>(),
					PIB = cNode["PIB"].As<string>(),
					Address = cNode["Address"].As<string>(),
					ContactEmail = cNode["ContactEmail"].As<string>(),
					CompanyType = (CompanyType)cNode["CompanyType"].As<int>(),
					CompanyStatus = (CompanyStatus)cNode["CompanyStatus"].As<int>()
				};
			}

			return user;
		}

		private RefreshToken MapRefreshTokenNode(INode rtNode)
		{
			return new RefreshToken
			{
				Id = rtNode["Id"].As<int>(),
				Token = rtNode["Token"].As<string>(),
				Expires = ConvertNeo4jDateTime(rtNode["Expires"]),
				Created = ConvertNeo4jDateTime(rtNode["Created"]),
				CreatedByIp = rtNode["CreatedByIp"].As<string>(),
				UserId = rtNode["UserId"].As<int>(),
				Revoked = rtNode.Properties.ContainsKey("Revoked") ? (DateTime?)ConvertNeo4jDateTime(rtNode["Revoked"]) : null,
				RevokedByIp = rtNode.Properties.ContainsKey("RevokedByIp") ? rtNode["RevokedByIp"].As<string?>() : null,
				ReplacedByToken = rtNode.Properties.ContainsKey("ReplacedByToken") ? rtNode["ReplacedByToken"].As<string?>() : null
			};
		}


		public async Task<User> FindByRefreshTokenAsync(string token)
		{
			var query = @"
        MATCH (u:User)-[:HAS_REFRESH_TOKEN]->(rt:RefreshToken { Token: $Token })
        OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
        RETURN u, c
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Token = token });
				var record = (await result.ToListAsync()).FirstOrDefault();

				if (record == null)
					throw new Exception("User not found for refresh token");

				var uNode = record["u"].As<INode>();
				var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;

				return MapUserNode(uNode, cNode);
			}
			finally
			{
				await session.CloseAsync();
			}
		}


		private DateTime ConvertNeo4jDateTime(object value)
		{
			if (value is ZonedDateTime zdt)
				return zdt.ToDateTimeOffset().UtcDateTime;
			if (value is LocalDateTime ldt)
				return ldt.ToDateTime();
			if (value is DateTime dt)
				return dt;

			throw new Exception($"Unsupported date type from Neo4j: {value.GetType()}");
		}

		public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
		{
			var query = @"
        MATCH (rt:RefreshToken { Token: $Token })
        SET rt.Revoked = datetime(),
            rt.RevokedByIp = $IpAddress
        RETURN rt
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Token = token, IpAddress = ipAddress });
				await result.ConsumeAsync(); // izvršava upit
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<User>> GetAllAsync(UserSearchDto userSearch)
		{
			var allowedSortFields = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
	{
		{ "Id", "u.Id" },
		{ "Email", "u.Email" },
		{ "FirstName", "u.FirstName" },
		{ "LastName", "u.LastName" },
		{ "CreatedAt", "u.CreatedAt" },
		{ "LastLogin", "u.LastLogin" }
	};

			var sortField = allowedSortFields.ContainsKey(userSearch.SortBy ?? "")
				? allowedSortFields[userSearch.SortBy!]
				: "u.Id";

			var sortDirection = userSearch.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";

			var skip = (userSearch.Page - 1) * userSearch.PageSize;
			var limit = userSearch.PageSize;

			// Kreiranje WHERE klauzule za filtere
			var filters = new List<string>();
			var parameters = new Dictionary<string, object>();

			if (!string.IsNullOrWhiteSpace(userSearch.Email))
			{
				filters.Add("toLower(u.Email) CONTAINS toLower($Email)");
				parameters["Email"] = userSearch.Email;
			}

			if (!string.IsNullOrWhiteSpace(userSearch.FirstName))
			{
				filters.Add("toLower(u.FirstName) CONTAINS toLower($FirstName)");
				parameters["FirstName"] = userSearch.FirstName;
			}

			if (!string.IsNullOrWhiteSpace(userSearch.LastName))
			{
				filters.Add("toLower(u.LastName) CONTAINS toLower($LastName)");
				parameters["LastName"] = userSearch.LastName;
			}

			var whereClause = filters.Count > 0 ? "WHERE " + string.Join(" AND ", filters) : "";

			var query = $@"
        MATCH (u:User)
        OPTIONAL MATCH (u)-[:BELONGS_TO]->(c:Company)
        {whereClause}
        RETURN u, c
        ORDER BY {sortField} {sortDirection}
        SKIP $skip
        LIMIT $limit
    ";

			parameters["skip"] = skip;
			parameters["limit"] = limit;

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, parameters);
				var records = await result.ToListAsync();

				var users = records.Select(record =>
				{
					var uNode = record["u"].As<INode>();
					var cNode = record.ContainsKey("c") ? record["c"].As<INode>() : null;
					return MapUserNode(uNode, cNode);
				}).ToList();

				return users;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

	}
}
