using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class CargoTypeNeo4jRepository
	{
		private readonly IDriver _driver;

		public CargoTypeNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(CargoType cargoType)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                CREATE (c:CargoType {
                    Id: $id,
                    TypeName: $typeName,
                    Description: $description,
                    HazardLevel: $hazardLevel,
                    RequiresSpecialEquipment: $requiresSpecialEquipment
                })";

			var parameters = new Dictionary<string, object>
			{
				["id"] = cargoType.Id,
				["typeName"] = cargoType.TypeName,
				["description"] = cargoType.Description ?? "",
				["hazardLevel"] = cargoType.HazardLevel,
				["requiresSpecialEquipment"] = cargoType.RequiresSpecialEquipment
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task<IEnumerable<CargoType>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:CargoType) RETURN c";

			var result = await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<CargoType>();
				foreach (var record in records)
				{
					var node = record["c"].As<INode>();

					var cargoType = new CargoType
					{
						Id = (int)(long)node.Properties["Id"], // Neo4j int comes as long
						TypeName = (string)node.Properties["TypeName"],
						Description = node.Properties.ContainsKey("Description") ? (string)node.Properties["Description"] : null,
						HazardLevel = (int)(long)node.Properties["HazardLevel"],
						RequiresSpecialEquipment = (bool)node.Properties["RequiresSpecialEquipment"]
					};

					list.Add(cargoType);
				}
				return list;
			});

			return result;
		}

		public async Task<CargoType?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:CargoType {Id: $id}) RETURN c LIMIT 1";

			var result = await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				var hasRecord = await cursor.FetchAsync();
				if (!hasRecord) return null;

				var record = cursor.Current;
				var node = record["c"].As<INode>();

				return new CargoType
				{
					Id = (int)(long)node.Properties["Id"],
					TypeName = (string)node.Properties["TypeName"],
					Description = node.Properties.ContainsKey("Description") ? (string)node.Properties["Description"] : null,
					HazardLevel = (int)(long)node.Properties["HazardLevel"],
					RequiresSpecialEquipment = (bool)node.Properties["RequiresSpecialEquipment"]
				};
			});

			return result;
		}

		public async Task UpdateAsync(CargoType cargoType)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (c:CargoType {Id: $id})
                SET c.TypeName = $typeName,
                    c.Description = $description,
                    c.HazardLevel = $hazardLevel,
                    c.RequiresSpecialEquipment = $requiresSpecialEquipment";

			var parameters = new Dictionary<string, object>
			{
				["id"] = cargoType.Id,
				["typeName"] = cargoType.TypeName,
				["description"] = cargoType.Description ?? "",
				["hazardLevel"] = cargoType.HazardLevel,
				["requiresSpecialEquipment"] = cargoType.RequiresSpecialEquipment
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (c:CargoType {Id: $id}) DETACH DELETE c";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}
	}
}
