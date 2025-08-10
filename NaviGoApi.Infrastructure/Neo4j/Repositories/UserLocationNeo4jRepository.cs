using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class UserLocationNeo4jRepository : IUserLocationRepository
	{
		private readonly IDriver _driver;

		public UserLocationNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(UserLocation location)
		{
			var id = await GetNextIdAsync("UserLocation");

			var query = @"
            CREATE (ul:UserLocation {
                id: $id,
                userId: $userId,
                ipAddress: $ipAddress,
                region: $region,
                accessTime: datetime($accessTime)
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					userId = location.UserId,
					ipAddress = location.IpAddress,
					region = location.Region,
					accessTime = location.AccessTime.ToString("o")
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<List<UserLocation>> GetRecentLocationsAsync(int userId, TimeSpan interval)
		{
			// Odredi vreme granicu
			var cutoff = DateTime.UtcNow.Subtract(interval).ToString("o");

			var query = @"
				MATCH (ul:UserLocation)
				WHERE ul.userId = $userId AND ul.accessTime >= datetime($cutoff)
				RETURN ul
				ORDER BY ul.accessTime DESC";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { userId, cutoff });
				var locations = new List<UserLocation>();

				await result.ForEachAsync(record =>
				{
					var node = record["ul"].As<INode>();
					locations.Add(NodeToEntity(node));
				});

				return locations;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public Task SaveChangesAsync()
		{
			// U Neo4j nema potrebe za ovim, jer je svaki RunAsync poziv transakcija i odmah se čuva
			return Task.CompletedTask;
		}

		private UserLocation NodeToEntity(INode node)
		{
			return new UserLocation
			{
				Id = node.Properties.ContainsKey("id") ? Convert.ToInt32(node.Properties["id"]) : 0,
				UserId = node.Properties.ContainsKey("userId") ? Convert.ToInt32(node.Properties["userId"]) : 0,
				IpAddress = node.Properties.ContainsKey("ipAddress") ? node.Properties["ipAddress"].ToString()! : string.Empty,
				Region = node.Properties.ContainsKey("region") ? node.Properties["region"].ToString()! : string.Empty,
				AccessTime = node.Properties.ContainsKey("accessTime")
					? DateTime.Parse(node.Properties["accessTime"].ToString()!)
					: DateTime.MinValue
			};
		}
	}
}
