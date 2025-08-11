using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class LocationNeo4jRepository : ILocationRepository
	{
		private readonly IDriver _driver;

		public LocationNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(Location location)
		{
			var id = await GetNextIdAsync("Location");

			await using var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(@"
                CREATE (l:Location {
                    Id: $Id,
                    City: $City,
                    Country: $Country,
                    ZIP: $ZIP,
                    Latitude: $Latitude,
                    Longitude: $Longitude,
                    FullAddress: $FullAddress
                })",
					new
					{
						Id = id,
						location.City,
						location.Country,
						location.ZIP,
						location.Latitude,
						location.Longitude,
						location.FullAddress
					});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (l:Location {Id: $Id})
				DETACH DELETE l",
				new { Id = id });
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync("MATCH (l:Location) RETURN l");
			var records = await cursor.ToListAsync();
			return records.Select(r => MapNodeToEntity(r["l"].As<INode>())).ToList();
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();
			var cursor = await session.RunAsync(@"
				MATCH (l:Location {Id: $Id})
				RETURN l",
				new { Id = id });

			var records = await cursor.ToListAsync();
			if (records.Count == 0)
				return null;

			return MapNodeToEntity(records[0]["l"].As<INode>());
		}

		public async Task UpdateAsync(Location location)
		{
			await using var session = _driver.AsyncSession();
			await session.RunAsync(@"
				MATCH (l:Location {Id: $Id})
				SET l.City = $City,
					l.Country = $Country,
					l.ZIP = $ZIP,
					l.Latitude = $Latitude,
					l.Longitude = $Longitude,
					l.FullAddress = $FullAddress",
				new
				{
					location.Id,
					location.City,
					location.Country,
					location.ZIP,
					location.Latitude,
					location.Longitude,
					location.FullAddress
				});
		}

		private Location MapNodeToEntity(INode node)
		{
			return new Location
			{
				Id = node.Properties["Id"].As<int>(),
				City = node.Properties["City"].As<string>(),
				Country = node.Properties["Country"].As<string>(),
				ZIP = node.Properties["ZIP"].As<string>(),
				Latitude = node.Properties["Latitude"].As<double>(),
				Longitude = node.Properties["Longitude"].As<double>(),
				FullAddress = node.Properties["FullAddress"].As<string>()
			};
		}

		public Task<Location?> GetByFullLocationAsync(string zip, string fullAddress, string city)
		{
			throw new NotImplementedException();
		}
	}
}
