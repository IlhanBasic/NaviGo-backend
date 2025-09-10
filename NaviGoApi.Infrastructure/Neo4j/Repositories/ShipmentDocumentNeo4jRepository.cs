using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Common.DTOs;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentDocumentNeo4jRepository : IShipmentDocumentRepository
	{
		private readonly IDriver _driver;

		public ShipmentDocumentNeo4jRepository(IDriver driver)
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

		public async Task AddAsync(ShipmentDocument document)
		{
			var id = await GetNextIdAsync("ShipmentDocument");

			var query = @"
            CREATE (d:ShipmentDocument {
                id: $id,
                shipmentId: $shipmentId,
                documentType: $documentType,
                fileUrl: $fileUrl,
                uploadDate: datetime($uploadDate),
                verified: $verified,
                verifiedByUserId: $verifiedByUserId
            })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = id,
					shipmentId = document.ShipmentId,
					documentType = (int)document.DocumentType,
					fileUrl = document.FileUrl,
					uploadDate = document.UploadDate.ToString("o"),
					verified = false,
					verifiedByUserId = document.VerifiedByUserId
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task DeleteAsync(int id)
		{
			var query = "MATCH (d:ShipmentDocument {id: $id}) DETACH DELETE d";

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

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
		{
			var query = "MATCH (d:ShipmentDocument) RETURN d";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query);
				var records = await cursor.ToListAsync();

				var result = new List<ShipmentDocument>();

				foreach (var record in records)
				{
					var node = record["d"].As<INode>();
					var document = MapNodeToShipmentDocument(node);
					result.Add(document);
				}

				return result;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task<ShipmentDocument?> GetByIdAsync(int id)
		{
			var query = "MATCH (d:ShipmentDocument {id: $id}) RETURN d LIMIT 1";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { id });

				// Koristi MoveNextAsync i Current za dohvat rezultata
				if (await cursor.FetchAsync())
				{
					var record = cursor.Current;
					var node = record["d"].As<INode>();
					return MapNodeToShipmentDocument(node);
				}

				return null;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		public async Task UpdateAsync(ShipmentDocument document)
		{
			var query = @"
                MATCH (d:ShipmentDocument {id: $id})
                SET d.shipmentId = $shipmentId,
                    d.documentType = $documentType,
                    d.fileUrl = $fileUrl,
                    d.uploadDate = datetime($uploadDate),
                    d.verified = $verified,
                    d.verifiedByUserId = $verifiedByUserId,
					d.expiryDate=$expiryDate";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = document.Id,
					shipmentId = document.ShipmentId,
					documentType = (int)document.DocumentType,
					fileUrl = document.FileUrl,
					uploadDate = document.UploadDate.ToString("o"),
					verified = document.Verified,
					verifiedByUserId = document.VerifiedByUserId,
					expiryDate= (DateTime?)null
				});
			}
			finally
			{
				await session.CloseAsync();
			}
		}

		// Helper method to map Neo4j node properties to ShipmentDocument entity
		private ShipmentDocument MapNodeToShipmentDocument(INode node)
		{
			var uploadDateObj = node.Properties["uploadDate"];

			DateTime uploadDate;

			if (uploadDateObj is LocalDateTime ldt)
			{
				uploadDate = ldt.ToDateTime(); // konvertuje u DateTime
			}
			else if (uploadDateObj is DateTime dt)
			{
				uploadDate = dt;
			}
			else
			{
				// fallback
				uploadDate = DateTime.Parse(uploadDateObj.ToString());
			}

			return new ShipmentDocument
			{
				Id = node.Properties["id"].As<int>(),
				ShipmentId = node.Properties["shipmentId"].As<int>(),
				DocumentType = (DocumentType)node.Properties["documentType"].As<int>(),
				FileUrl = node.Properties["fileUrl"]?.ToString() ?? string.Empty,
				UploadDate = uploadDate,
				Verified = node.Properties["verified"].As<bool>(),
				VerifiedByUserId = node.Properties.ContainsKey("verifiedByUserId") && node.Properties["verifiedByUserId"] != null
					? node.Properties["verifiedByUserId"].As<int?>()
					: null
			};
		}


		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync(ShipmentDocumentSearchDto search)
		{
			string sortBy = search.SortBy?.ToLower() switch
			{
				"uploaddate" => "d.uploadDate",
				"verified" => "d.verified",
				_ => "d.id"
			};

			string sortDirection = search.SortDirection?.ToLower() == "desc" ? "DESC" : "ASC";

			int skip = (search.Page - 1) * search.PageSize;
			int limit = search.PageSize;

			var query = $@"
        MATCH (d:ShipmentDocument)
        RETURN d
        ORDER BY {sortBy} {sortDirection}
        SKIP $skip
        LIMIT $limit
    ";

			var session = _driver.AsyncSession();
			try
			{
				var cursor = await session.RunAsync(query, new { skip, limit });
				var records = await cursor.ToListAsync();

				var result = new List<ShipmentDocument>();
				foreach (var record in records)
				{
					var node = record["d"].As<INode>();
					result.Add(MapNodeToShipmentDocument(node));
				}

				return result;
			}
			finally
			{
				await session.CloseAsync();
			}
		}

	}
}
