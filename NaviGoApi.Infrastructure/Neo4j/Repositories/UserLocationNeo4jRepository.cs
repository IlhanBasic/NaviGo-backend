using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class UserLocationNeo4jRepository
	{
		private readonly IDriver _driver;

		public UserLocationNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(UserLocation location)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (u:User {Id: $userId})
                CREATE (loc:UserLocation {
                    Id: $id,
                    IpAddress: $ip,
                    Region: $region,
                    AccessTime: datetime($accessTime)
                })
                CREATE (loc)-[:BELONGS_TO]->(u)";

			var parameters = new Dictionary<string, object>
			{
				["id"] = location.Id,
				["userId"] = location.UserId,
				["ip"] = location.IpAddress,
				["region"] = location.Region,
				["accessTime"] = location.AccessTime.ToString("o")
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task<List<UserLocation>> GetRecentLocationsAsync(int userId, TimeSpan interval)
		{
			await using var session = _driver.AsyncSession();

			var cutoff = DateTime.UtcNow.Subtract(interval).ToString("o");

			var query = @"
                MATCH (loc:UserLocation)-[:BELONGS_TO]->(u:User {Id: $userId})
                WHERE loc.AccessTime >= datetime($cutoff)
                RETURN loc
                ORDER BY loc.AccessTime DESC";

			return await session.ReadTransactionAsync(async tx =>
			{
				var result = await tx.RunAsync(query, new { userId, cutoff });
				var records = await result.ToListAsync();

				var list = new List<UserLocation>();

				foreach (var record in records)
				{
					var node = record["loc"].As<INode>();
					list.Add(new UserLocation
					{
						Id = (int)(long)node.Properties["Id"],
						UserId = userId,
						IpAddress = (string)node.Properties["IpAddress"],
						Region = (string)node.Properties["Region"],
						AccessTime = DateTime.Parse((string)node.Properties["AccessTime"])
					});
				}

				return list;
			});
		}
	}
}
