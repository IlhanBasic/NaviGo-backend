using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	internal class VehicleTypeNeo4jRepository : IVehicleTypeRepository
	{
		private readonly IDriver _driver;

		public VehicleTypeNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(VehicleType vehicleType)
		{
			var query = @"
                CREATE (vt:VehicleType {
                    Id: $id,
                    TypeName: $typeName,
                    Description: $description,
                    RequiresSpecialLicense: $requiresSpecialLicense
                })";

			var parameters = new Dictionary<string, object>
			{
				{ "id", vehicleType.Id },
				{ "typeName", vehicleType.TypeName },
				{ "description", vehicleType.Description ?? "" },
				{ "requiresSpecialLicense", vehicleType.RequiresSpecialLicense }
			};

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = @"
                MATCH (vt:VehicleType {Id: $id})
                DETACH DELETE vt";

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
			var result = new List<VehicleType>();
			try
			{
				var cursor = await session.RunAsync(query);
				while (await cursor.FetchAsync())
				{
					var node = cursor.Current["vt"].As<INode>();
					result.Add(MapNodeToVehicleType(node));
				}
			}
			finally
			{
				await session.CloseAsync();
			}
			return result;
		}

		public async Task<VehicleType?> GetByIdAsync(int id)
		{
			var query = @"
                MATCH (vt:VehicleType {Id: $id})
                RETURN vt
                LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });
				if (await cursor.FetchAsync())
				{
					var node = cursor.Current["vt"].As<INode>();
					return MapNodeToVehicleType(node);
				}
				return null;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(VehicleType vehicleType)
		{
			var query = @"
                MATCH (vt:VehicleType {Id: $id})
                SET vt.TypeName = $typeName,
                    vt.Description = $description,
                    vt.RequiresSpecialLicense = $requiresSpecialLicense";

			var parameters = new Dictionary<string, object>
			{
				{ "id", vehicleType.Id },
				{ "typeName", vehicleType.TypeName },
				{ "description", vehicleType.Description ?? "" },
				{ "requiresSpecialLicense", vehicleType.RequiresSpecialLicense }
			};

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, parameters);
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		private VehicleType MapNodeToVehicleType(INode node)
		{
			return new VehicleType
			{
				Id = (int)(long)node.Properties["Id"],
				TypeName = node.Properties["TypeName"].As<string>(),
				Description = node.Properties.ContainsKey("Description") ? node.Properties["Description"].As<string>() : null,
				RequiresSpecialLicense = node.Properties["RequiresSpecialLicense"].As<bool>()
			};
		}
	}
}
