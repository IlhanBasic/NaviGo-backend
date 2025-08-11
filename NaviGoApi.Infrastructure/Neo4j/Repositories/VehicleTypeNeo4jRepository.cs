using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq.Expressions;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class VehicleTypeNeo4jRepository : IVehicleTypeRepository
	{
		private readonly IDriver _driver;

		public VehicleTypeNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(VehicleType vehicleType)
		{
			var id = await GetNextIdAsync("VehicleType");

			var query = @"
            CREATE (vt:VehicleType {
                id: $id,
                typeName: $typeName,
                description: $description,
                requiresSpecialLicense: $requiresSpecialLicense
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					vehicleType.TypeName,
					description = vehicleType.Description ?? "",
					vehicleType.RequiresSpecialLicense
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"MATCH (vt:VehicleType {id: $id}) DETACH DELETE vt";

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

		public async Task<IEnumerable<VehicleType>> GetAllAsync()
		{
			var query = @"MATCH (vt:VehicleType) RETURN vt";

			var session = _driver.AsyncSession();
			var list = new List<VehicleType>();
			try
			{
				var result = await session.RunAsync(query);
				await result.ForEachAsync(record =>
				{
					var node = record["vt"].As<INode>();
					list.Add(NodeToEntity(node));
				});
				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<VehicleType?> GetByIdAsync(int id)
		{
			var query = @"MATCH (vt:VehicleType {id: $id}) RETURN vt LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { id });
				var found = await result.FetchAsync();
				if (!found) return null;

				var node = result.Current["vt"].As<INode>();
				return NodeToEntity(node);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(VehicleType vehicleType)
		{
			var query = @"
				MATCH (vt:VehicleType {id: $id})
				SET vt.typeName = $typeName,
					vt.description = $description,
					vt.requiresSpecialLicense = $requiresSpecialLicense";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = vehicleType.Id,
					typeName = vehicleType.TypeName,
					description = vehicleType.Description ?? "",
					requiresSpecialLicense = vehicleType.RequiresSpecialLicense
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private VehicleType NodeToEntity(INode node)
		{
			return new VehicleType
			{
				Id = node.Properties.ContainsKey("id") ? Convert.ToInt32(node.Properties["id"]) : 0,
				TypeName = node.Properties.ContainsKey("typeName") ? node.Properties["typeName"].ToString()! : string.Empty,
				Description = node.Properties.ContainsKey("description") ? node.Properties["description"].ToString() : null,
				RequiresSpecialLicense = node.Properties.ContainsKey("requiresSpecialLicense") && (bool)node.Properties["requiresSpecialLicense"]
			};
		}

		public Task<bool> ExistsAsync(Expression<Func<VehicleType, bool>> predicate)
		{
			throw new NotImplementedException();
		}
	}
}
