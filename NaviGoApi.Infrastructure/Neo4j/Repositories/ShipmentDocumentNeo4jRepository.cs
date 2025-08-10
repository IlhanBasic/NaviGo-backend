using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentDocumentNeo4jRepository : IShipmentDocumentRepository
	{
		private readonly IDriver _driver;

		public ShipmentDocumentNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(ShipmentDocument document)
		{
			var query = @"
                CREATE (d:ShipmentDocument {
                    id: $id,
                    shipmentId: $shipmentId,
                    documentType: $documentType,
                    fileUrl: $fileUrl,
                    uploadDate: datetime($uploadDate),
                    verified: $verified,
                    verifiedByUserId: $verifiedByUserId,
                    expiryDate: $expiryDate
                })";

			var session = _driver.AsyncSession();
			try
			{
				await session.RunAsync(query, new
				{
					id = document.Id,
					shipmentId = document.ShipmentId,
					documentType = (int)document.DocumentType,
					fileUrl = document.FileUrl,
					uploadDate = document.UploadDate.ToString("o"), // ISO 8601 format
					verified = document.Verified,
					verifiedByUserId = document.VerifiedByUserId,
					expiryDate = document.ExpiryDate?.ToString("o")
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
                    d.expiryDate = $expiryDate";

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
					expiryDate = document.ExpiryDate?.ToString("o")
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
			return new ShipmentDocument
			{
				Id = Convert.ToInt32(node.Properties["id"]),
				ShipmentId = Convert.ToInt32(node.Properties["shipmentId"]),
				DocumentType = (DocumentType)Convert.ToInt32(node.Properties["documentType"]),
				FileUrl = node.Properties["fileUrl"]?.ToString() ?? string.Empty,
				UploadDate = DateTime.Parse(node.Properties["uploadDate"].ToString()),
				Verified = Convert.ToBoolean(node.Properties["verified"]),
				VerifiedByUserId = node.Properties.ContainsKey("verifiedByUserId") && node.Properties["verifiedByUserId"] != null
					? (int?)Convert.ToInt32(node.Properties["verifiedByUserId"])
					: null,
				ExpiryDate = node.Properties.ContainsKey("expiryDate") && node.Properties["expiryDate"] != null
					? (DateTime?)DateTime.Parse(node.Properties["expiryDate"].ToString())
					: null
			};
		}
	}
}
