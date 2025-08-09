using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class LocationNeo4jRepository
	{
		private readonly IDriver _driver;

		public LocationNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(Location location)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (loc:Location {
                    Id: $id,
                    City: $city,
                    Country: $country,
                    ZIP: $zip,
                    Latitude: $latitude,
                    Longitude: $longitude,
                    FullAddress: $fullAddress
                })
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = location.Id,
				["city"] = location.City,
				["country"] = location.Country,
				["zip"] = location.ZIP,
				["latitude"] = location.Latitude,
				["longitude"] = location.Longitude,
				["fullAddress"] = location.FullAddress
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (loc:Location {Id: $id})
                DETACH DELETE loc
            ";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<IEnumerable<Location>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (loc:Location)
                RETURN loc
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var locations = new List<Location>();

				foreach (var record in records)
				{
					var node = record["loc"].As<INode>();
					locations.Add(MapNodeToLocation(node));
				}

				return locations;
			});
		}

		public async Task<Location?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (loc:Location {Id: $id})
                RETURN loc
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();

				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["loc"].As<INode>();
				return MapNodeToLocation(node);
			});
		}

		public async Task UpdateAsync(Location location)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (loc:Location {Id: $id})
                SET loc.City = $city,
                    loc.Country = $country,
                    loc.ZIP = $zip,
                    loc.Latitude = $latitude,
                    loc.Longitude = $longitude,
                    loc.FullAddress = $fullAddress
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = location.Id,
				["city"] = location.City,
				["country"] = location.Country,
				["zip"] = location.ZIP,
				["latitude"] = location.Latitude,
				["longitude"] = location.Longitude,
				["fullAddress"] = location.FullAddress
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		private Location MapNodeToLocation(INode node)
		{
			return new Location
			{
				Id = (int)(long)node.Properties["Id"],
				City = (string)node.Properties["City"],
				Country = (string)node.Properties["Country"],
				ZIP = (string)node.Properties["ZIP"],
				Latitude = Convert.ToDouble(node.Properties["Latitude"]),
				Longitude = Convert.ToDouble(node.Properties["Longitude"]),
				FullAddress = (string)node.Properties["FullAddress"]
			};
		}
	}
}
