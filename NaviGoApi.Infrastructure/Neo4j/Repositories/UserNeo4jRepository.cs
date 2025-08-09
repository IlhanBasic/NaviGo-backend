using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class UserNeo4jRepository : IDisposable
	{
		private readonly IDriver _driver;

		public UserNeo4jRepository(string uri, string user, string password)
		{
			_driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
		}

		public async Task AddAsync(User user)
		{
			var query = @"
                CREATE (u:User {
                    Id: $id,
                    Email: $email,
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
					firstName = user.FirstName,
					lastName = user.LastName,
					phoneNumber = user.PhoneNumber,
					userRole = (int)user.UserRole,
					createdAt = user.CreatedAt.ToString("o"), // ISO 8601 format
					emailVerified = user.EmailVerified,
					userStatus = (int)user.UserStatus
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<User?> GetByIdAsync(int id)
		{
			var query = @"
        MATCH (u:User {Id: $id})
        RETURN u";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });
				var record = await cursor.FetchAsync() ? cursor.Current : null;

				if (record == null)
					return null;

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
			var query = "MATCH (u:User) RETURN u";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query);
				var users = new List<User>();

				await cursor.ForEachAsync(record =>
				{
					var node = record["u"].As<INode>();
					users.Add(MapNodeToUser(node));
				});

				return users;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(User user)
		{
			var query = @"
                MATCH (u:User {Id: $id})
                SET u.Email = $email,
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

		public async Task DeleteAsync(int id)
		{
			var query = "MATCH (u:User {Id: $id}) DETACH DELETE u";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private User MapNodeToUser(INode node)
		{
			return new User
			{
				Id = Convert.ToInt32(node.Properties["Id"]),
				Email = node.Properties["Email"].ToString() ?? "",
				FirstName = node.Properties["FirstName"].ToString() ?? "",
				LastName = node.Properties["LastName"].ToString() ?? "",
				PhoneNumber = node.Properties["PhoneNumber"].ToString() ?? "",
				UserRole = (UserRole)(int)(long)node.Properties["UserRole"], // Neo4j long -> int -> enum
				CreatedAt = DateTime.Parse(node.Properties["CreatedAt"].ToString()!),
				EmailVerified = (bool)node.Properties["EmailVerified"],
				UserStatus = (UserStatus)(int)(long)node.Properties["UserStatus"]
			};
		}

		public void Dispose()
		{
			_driver?.Dispose();
		}
	}
}
