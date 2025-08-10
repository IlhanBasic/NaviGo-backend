using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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
			var query = @"
				CREATE (u:User {
					Id: $id,
					Email: $email,
					PasswordHash: $passwordHash,
					FirstName: $firstName,
					LastName: $lastName,
					PhoneNumber: $phoneNumber,
					UserRole: $userRole,
					CreatedAt: datetime($createdAt),
					EmailVerified: $emailVerified,
					UserStatus: $userStatus
				})";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = user.Id,
					email = user.Email,
					passwordHash = user.PasswordHash,
					firstName = user.FirstName,
					lastName = user.LastName,
					phoneNumber = user.PhoneNumber,
					userRole = (int)user.UserRole,
					createdAt = user.CreatedAt,
					emailVerified = user.EmailVerified,
					userStatus = (int)user.UserStatus
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task AddRefreshTokenAsync(RefreshToken token)
		{
			var query = @"
				MATCH (u:User {Id: $userId})
				CREATE (rt:RefreshToken {
					Id: $id,
					Token: $token,
					Expires: datetime($expires),
					Created: datetime($created),
					CreatedByIp: $createdByIp,
					Revoked: $revoked,
					RevokedByIp: $revokedByIp,
					ReplacedByToken: $replacedByToken
				})
				CREATE (u)-[:HAS_REFRESH_TOKEN]->(rt)";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					userId = token.UserId,
					id = token.Id,
					token = token.Token,
					expires = token.Expires,
					created = token.Created,
					createdByIp = token.CreatedByIp,
					revoked = token.Revoked.HasValue ? (object)token.Revoked.Value : null,
					revokedByIp = token.RevokedByIp,
					replacedByToken = token.ReplacedByToken
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(User user)
		{
			var query = @"
				MATCH (u:User {Id: $id})
				DETACH DELETE u";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { id = user.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> FindByRefreshTokenAsync(string token)
		{
			var query = @"
				MATCH (u:User)-[:HAS_REFRESH_TOKEN]->(rt:RefreshToken {Token: $token})
				RETURN u LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { token });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["u"].As<INode>();
				return MapNodeToUser(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<User>> GetAllAsync()
		{
			var query = @"MATCH (u:User) RETURN u";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var users = new List<User>();
				while (await result.FetchAsync())
				{
					var record = result.Current;
					var node = record["u"].As<INode>();
					users.Add(MapNodeToUser(node));
				}
				return users;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByEmailAsync(string email)
		{
			var query = @"MATCH (u:User {Email: $email}) RETURN u LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { email });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["u"].As<INode>();
				return MapNodeToUser(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByEmailVerificationTokenAsync(string token)
		{
			var query = @"MATCH (u:User {EmailVerificationToken: $token}) RETURN u LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { token });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["u"].As<INode>();
				return MapNodeToUser(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			var query = @"MATCH (u:User {Id: $id}) RETURN u LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { id });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["u"].As<INode>();
				return MapNodeToUser(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByPasswordResetTokenAsync(string token)
		{
			var query = @"MATCH (u:User {PasswordResetToken: $token}) RETURN u LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { token });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["u"].As<INode>();
				return MapNodeToUser(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
		{
			var query = @"MATCH (rt:RefreshToken {Token: $token}) RETURN rt LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { token });
				var hasRecord = await result.FetchAsync();
				if (!hasRecord)
					return null;

				var record = result.Current;
				var node = record["rt"].As<INode>();
				return MapNodeToRefreshToken(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public IQueryable<User> Query()
		{
			throw new NotSupportedException("IQueryable is not supported in Neo4j repository.");
		}

		public async Task UpdateAsync(User user)
		{
			var query = @"
				MATCH (u:User {Id: $id})
				SET u.Email = $email,
					u.PasswordHash = $passwordHash,
					u.FirstName = $firstName,
					u.LastName = $lastName,
					u.PhoneNumber = $phoneNumber,
					u.UserRole = $userRole,
					u.EmailVerified = $emailVerified,
					u.UserStatus = $userStatus";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = user.Id,
					email = user.Email,
					passwordHash = user.PasswordHash,
					firstName = user.FirstName,
					lastName = user.LastName,
					phoneNumber = user.PhoneNumber,
					userRole = (int)user.UserRole,
					emailVerified = user.EmailVerified,
					userStatus = (int)user.UserStatus
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateRefreshTokenAsync(RefreshToken token)
		{
			var query = @"
				MATCH (rt:RefreshToken {Id: $id})
				SET rt.Token = $token,
					rt.Expires = datetime($expires),
					rt.Created = datetime($created),
					rt.CreatedByIp = $createdByIp,
					rt.Revoked = $revoked,
					rt.RevokedByIp = $revokedByIp,
					rt.ReplacedByToken = $replacedByToken";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = token.Id,
					token = token.Token,
					expires = token.Expires,
					created = token.Created,
					createdByIp = token.CreatedByIp,
					revoked = token.Revoked.HasValue ? (object)token.Revoked.Value : null,
					revokedByIp = token.RevokedByIp,
					replacedByToken = token.ReplacedByToken
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		// --- Helper metode za mapiranje nodova u entitete ---
		private User MapNodeToUser(INode node)
		{
			return new User
			{
				Id = Convert.ToInt32(node.Properties["Id"]),
				Email = node.Properties["Email"].As<string>(),
				PasswordHash = node.Properties["PasswordHash"].As<string>(),
				FirstName = node.Properties["FirstName"].As<string>(),
				LastName = node.Properties["LastName"].As<string>(),
				PhoneNumber = node.Properties.ContainsKey("PhoneNumber") ? node.Properties["PhoneNumber"].As<string>() : null,
				UserRole = (UserRole)Convert.ToInt32(node.Properties["UserRole"]),
				CreatedAt = node.Properties.ContainsKey("CreatedAt") ? DateTime.Parse(node.Properties["CreatedAt"].ToString()) : DateTime.UtcNow,
				EmailVerified = node.Properties.ContainsKey("EmailVerified") && (bool)node.Properties["EmailVerified"],
				UserStatus = node.Properties.ContainsKey("UserStatus") ? (UserStatus)Convert.ToInt32(node.Properties["UserStatus"]) : UserStatus.Active,
				EmailVerificationToken = node.Properties.ContainsKey("EmailVerificationToken") ? node.Properties["EmailVerificationToken"].As<string>() : null,
				PasswordResetToken = node.Properties.ContainsKey("PasswordResetToken") ? node.Properties["PasswordResetToken"].As<string>() : null
			};
		}

		private RefreshToken MapNodeToRefreshToken(INode node)
		{
			return new RefreshToken
			{
				Id = Convert.ToInt32(node.Properties["Id"]),
				Token = node.Properties["Token"].As<string>(),
				Expires = node.Properties.ContainsKey("Expires") ? DateTime.Parse(node.Properties["Expires"].ToString()) : DateTime.MinValue,
				Created = node.Properties.ContainsKey("Created") ? DateTime.Parse(node.Properties["Created"].ToString()) : DateTime.MinValue,
				CreatedByIp = node.Properties.ContainsKey("CreatedByIp") ? node.Properties["CreatedByIp"].As<string>() : null!,
				Revoked = node.Properties.ContainsKey("Revoked") ? (DateTime?)DateTime.Parse(node.Properties["Revoked"].ToString()) : null,
				RevokedByIp = node.Properties.ContainsKey("RevokedByIp") ? node.Properties["RevokedByIp"].As<string>() : null,
				ReplacedByToken = node.Properties.ContainsKey("ReplacedByToken") ? node.Properties["ReplacedByToken"].As<string>() : null,
			};
		}
	}
}
