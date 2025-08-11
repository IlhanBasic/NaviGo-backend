using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Linq.Expressions;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class CargoTypeNeo4jRepository : ICargoTypeRepository
	{
		private readonly IDriver _driver;

		public CargoTypeNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(CargoType cargoType)
		{
			var id = await GetNextIdAsync("CargoType");

			var query = @"
        CREATE (ct:CargoType { 
            Id: $Id, 
            TypeName: $TypeName, 
            Description: $Description, 
            HazardLevel: $HazardLevel, 
            RequiresSpecialEquipment: $RequiresSpecialEquipment 
        })
    ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = id,
					TypeName = cargoType.TypeName,
					Description = cargoType.Description,
					HazardLevel = cargoType.HazardLevel,
					RequiresSpecialEquipment = cargoType.RequiresSpecialEquipment
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



		public async Task DeleteAsync(CargoType cargoType)
		{
			var query = @"
                MATCH (ct:CargoType { Id: $Id })
                DETACH DELETE ct
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new { Id = cargoType.Id });
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<IEnumerable<CargoType>> GetAllAsync()
		{
			var query = @"MATCH (ct:CargoType) RETURN ct";
			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query);
				var list = new List<CargoType>();

				await result.ForEachAsync(record =>
				{
					var node = record["ct"].As<INode>();
					list.Add(new CargoType
					{
						Id = node["Id"].As<int>(),
						TypeName = node["TypeName"].As<string>(),
						Description = node["Description"].As<string?>(),
						HazardLevel = node["HazardLevel"].As<int>(),
						RequiresSpecialEquipment = node["RequiresSpecialEquipment"].As<bool>()
					});
				});

				return list;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<CargoType?> GetByIdAsync(int id)
		{
			var query = @"
        MATCH (ct:CargoType { Id: $Id })
        RETURN ct
    ";

			var session = _driver.AsyncSession();
			try
			{
				var result = await session.RunAsync(query, new { Id = id });
				var records = await result.ToListAsync();
				var record = records.FirstOrDefault();

				if (record == null)
					return null;

				var node = record["ct"].As<INode>();
				return new CargoType
				{
					Id = node["Id"].As<int>(),
					TypeName = node["TypeName"].As<string>(),
					Description = node["Description"].As<string?>(),
					HazardLevel = node["HazardLevel"].As<int>(),
					RequiresSpecialEquipment = node["RequiresSpecialEquipment"].As<bool>()
				};
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(CargoType cargoType)
		{
			var query = @"
                MATCH (ct:CargoType { Id: $Id })
                SET ct.TypeName = $TypeName,
                    ct.Description = $Description,
                    ct.HazardLevel = $HazardLevel,
                    ct.RequiresSpecialEquipment = $RequiresSpecialEquipment
            ";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					Id = cargoType.Id,
					TypeName = cargoType.TypeName,
					Description = cargoType.Description,
					HazardLevel = cargoType.HazardLevel,
					RequiresSpecialEquipment = cargoType.RequiresSpecialEquipment
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public Task<bool> ExistsAsync(Expression<Func<CargoType, bool>> predicate)
		{
			throw new NotImplementedException();
		}
	}
}
