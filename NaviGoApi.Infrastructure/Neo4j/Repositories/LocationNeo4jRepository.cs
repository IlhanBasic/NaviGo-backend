

using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using Neo4j.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class LocationNeo4jRepository : ILocationRepository
	{
		private readonly IDriver _driver;

		public LocationNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Location location)
		{
			var id = await GetNextIdAsync("Location");

			var query = @"
                CREATE (l:Location {
                    Id: $Id,
                    City: $City,
                    Country: $Country,
                    ZIP: $ZIP,
                    Latitude: $Latitude,
                    Longitude: $Longitude,
                    FullAddress: $FullAddress
                })
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
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
			var query = @"MATCH (l:Location { Id: $Id }) DETACH DELETE l";
			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { Id = id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			var query = @"MATCH (l:Location) RETURN l";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var records = await result.ToListAsync();
				return records.Select(r => MapLocationNode(r["l"].As<INode>())).ToList();
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			var query = @"MATCH (l:Location { Id: $Id }) RETURN l";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				return MapLocationNode(record["l"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<Location?> GetByFullLocationAsync(string zip, string fullAddress, string city)
		{
			var query = @"
                MATCH (l:Location)
                WHERE l.ZIP = $ZIP AND l.FullAddress = $FullAddress AND l.City = $City
                RETURN l
            ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { ZIP = zip, FullAddress = fullAddress, City = city });
				var record = (await result.ToListAsync()).FirstOrDefault();
				if (record == null) return null;

				return MapLocationNode(record["l"].As<INode>());
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(Location location)
		{
			var query = @"
                MATCH (l:Location { Id: $Id })
                SET l.City = $City,
                    l.Country = $Country,
                    l.ZIP = $ZIP,
                    l.Latitude = $Latitude,
                    l.Longitude = $Longitude,
                    l.FullAddress = $FullAddress
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
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
			finally
			{
				await session.CloseAsync();
			}
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

		private Location MapLocationNode(INode node)
		{
			return new Location
			{
				Id = node["Id"].As<int>(),
				City = node["City"].As<string>(),
				Country = node["Country"].As<string>(),
				ZIP = node["ZIP"].As<string>(),
				Latitude = node["Latitude"].As<double>(),
				Longitude = node["Longitude"].As<double>(),
				FullAddress = node["FullAddress"].As<string>()
			};
		}
	}
}
