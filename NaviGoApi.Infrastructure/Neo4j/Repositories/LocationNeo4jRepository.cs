//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Neo4j.Driver;
//using NaviGoApi.Domain.Entities;
//using NaviGoApi.Domain.Interfaces;

//namespace NaviGoApi.Infrastructure.Neo4j.Repositories
//{
//	public class LocationNeo4jRepository : ILocationRepository
//	{
//		private readonly IDriver _driver;

//		public LocationNeo4jRepository(IDriver driver)
//		{
//			_driver = driver;
//		}

//		private async Task<int> GetNextIdAsync(string entityName)
//		{
//			var query = @"
//            MERGE (c:Counter { name: $entityName })
//            ON CREATE SET c.lastId = 1
//            ON MATCH SET c.lastId = c.lastId + 1
//            RETURN c.lastId as lastId
//        ";

//			var session = _driver.AsyncSession();
//			try
//			{
//				var result = await session.RunAsync(query, new { entityName });
//				var record = await result.SingleAsync();
//				return record["lastId"].As<int>();
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task AddAsync(Location location)
//		{
//			var id = await GetNextIdAsync("Location");

//			await using var session = _driver.AsyncSession();
//			try
//			{
//				await session.RunAsync(@"
//                CREATE (l:Location {
//                    Id: $Id,
//                    City: $City,
//                    Country: $Country,
//                    ZIP: $ZIP,
//                    Latitude: $Latitude,
//                    Longitude: $Longitude,
//                    FullAddress: $FullAddress
//                })",
//					new
//					{
//						Id = id,
//						location.City,
//						location.Country,
//						location.ZIP,
//						location.Latitude,
//						location.Longitude,
//						location.FullAddress
//					});
//			}
//			finally
//			{
//				await session.CloseAsync();
//			}
//		}

//		public async Task DeleteAsync(int id)
//		{
//			await using var session = _driver.AsyncSession();
//			await session.RunAsync(@"
//				MATCH (l:Location {Id: $Id})
//				DETACH DELETE l",
//				new { Id = id });
//		}

//		public async Task<IEnumerable<Location>> GetAllAsync()
//		{
//			await using var session = _driver.AsyncSession();
//			var cursor = await session.RunAsync("MATCH (l:Location) RETURN l");
//			var records = await cursor.ToListAsync();
//			return records.Select(r => MapNodeToEntity(r["l"].As<INode>())).ToList();
//		}

//		public async Task<Location?> GetByIdAsync(int id)
//		{
//			await using var session = _driver.AsyncSession();
//			var cursor = await session.RunAsync(@"
//				MATCH (l:Location {Id: $Id})
//				RETURN l",
//				new { Id = id });

//			var records = await cursor.ToListAsync();
//			if (records.Count == 0)
//				return null;

//			return MapNodeToEntity(records[0]["l"].As<INode>());
//		}

//		public async Task UpdateAsync(Location location)
//		{
//			await using var session = _driver.AsyncSession();
//			await session.RunAsync(@"
//				MATCH (l:Location {Id: $Id})
//				SET l.City = $City,
//					l.Country = $Country,
//					l.ZIP = $ZIP,
//					l.Latitude = $Latitude,
//					l.Longitude = $Longitude,
//					l.FullAddress = $FullAddress",
//				new
//				{
//					location.Id,
//					location.City,
//					location.Country,
//					location.ZIP,
//					location.Latitude,
//					location.Longitude,
//					location.FullAddress
//				});
//		}

//		private Location MapNodeToEntity(INode node)
//		{
//			return new Location
//			{
//				Id = node.Properties["Id"].As<int>(),
//				City = node.Properties["City"].As<string>(),
//				Country = node.Properties["Country"].As<string>(),
//				ZIP = node.Properties["ZIP"].As<string>(),
//				Latitude = node.Properties["Latitude"].As<double>(),
//				Longitude = node.Properties["Longitude"].As<double>(),
//				FullAddress = node.Properties["FullAddress"].As<string>()
//			};
//		}

//		public Task<Location?> GetByFullLocationAsync(string zip, string fullAddress, string city)
//		{
//			throw new NotImplementedException();
//		}
//	}
//}

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
