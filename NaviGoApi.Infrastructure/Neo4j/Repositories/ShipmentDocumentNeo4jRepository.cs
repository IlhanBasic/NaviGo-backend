using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Neo4j.Driver;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Neo4j.Repositories
{
	public class ShipmentDocumentNeo4jRepository
	{
		private readonly IDriver _driver;

		public ShipmentDocumentNeo4jRepository(IDriver driver)
		{
			_driver = driver;
		}

		public async Task AddAsync(ShipmentDocument document)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (s:Shipment {Id: $shipmentId})
                CREATE (doc:ShipmentDocument {
                    Id: $id,
                    DocumentType: $documentType,
                    FileUrl: $fileUrl,
                    UploadDate: datetime($uploadDate),
                    Verified: $verified,
                    VerifiedByUserId: $verifiedByUserId,
                    ExpiryDate: $expiryDate
                })
                CREATE (doc)-[:BELONGS_TO]->(s)
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = document.Id,
				["shipmentId"] = document.ShipmentId,
				["documentType"] = (int)document.DocumentType,
				["fileUrl"] = document.FileUrl,
				["uploadDate"] = document.UploadDate.ToString("o"), // ISO 8601 format
				["verified"] = document.Verified,
				["verifiedByUserId"] = document.VerifiedByUserId ?? 0,
				["expiryDate"] = document.ExpiryDate?.ToString("o") ?? null
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task UpdateAsync(ShipmentDocument document)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (doc:ShipmentDocument {Id: $id})
                SET doc.DocumentType = $documentType,
                    doc.FileUrl = $fileUrl,
                    doc.UploadDate = datetime($uploadDate),
                    doc.Verified = $verified,
                    doc.VerifiedByUserId = $verifiedByUserId,
                    doc.ExpiryDate = $expiryDate
            ";

			var parameters = new Dictionary<string, object>
			{
				["id"] = document.Id,
				["documentType"] = (int)document.DocumentType,
				["fileUrl"] = document.FileUrl,
				["uploadDate"] = document.UploadDate.ToString("o"),
				["verified"] = document.Verified,
				["verifiedByUserId"] = document.VerifiedByUserId ?? 0,
				["expiryDate"] = document.ExpiryDate?.ToString("o") ?? null
			};

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, parameters);
			});
		}

		public async Task DeleteAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = "MATCH (doc:ShipmentDocument {Id: $id}) DETACH DELETE doc";

			await session.WriteTransactionAsync(async tx =>
			{
				await tx.RunAsync(query, new { id });
			});
		}

		public async Task<ShipmentDocument?> GetByIdAsync(int id)
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (doc:ShipmentDocument {Id: $id})
                OPTIONAL MATCH (doc)-[:BELONGS_TO]->(s:Shipment)
                RETURN doc, s
                LIMIT 1
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query, new { id });
				if (!await cursor.FetchAsync()) return null;

				var record = cursor.Current;

				var docNode = record["doc"].As<INode>();
				var shipmentNode = record["s"]?.As<INode>();

				var document = new ShipmentDocument
				{
					Id = (int)(long)docNode.Properties["Id"],
					ShipmentId = shipmentNode != null ? (int)(long)shipmentNode.Properties["Id"] : 0,
					DocumentType = (DocumentType)(int)(long)docNode.Properties["DocumentType"],
					FileUrl = (string)docNode.Properties["FileUrl"],
					UploadDate = DateTime.Parse((string)docNode.Properties["UploadDate"]),
					Verified = (bool)docNode.Properties["Verified"],
					VerifiedByUserId = docNode.Properties.ContainsKey("VerifiedByUserId") ? (int?)(long)docNode.Properties["VerifiedByUserId"] : null,
					ExpiryDate = docNode.Properties.ContainsKey("ExpiryDate") && docNode.Properties["ExpiryDate"] != null
						? DateTime.Parse((string)docNode.Properties["ExpiryDate"])
						: null
				};

				if (shipmentNode != null)
				{
					document.Shipment = new Shipment { Id = (int)(long)shipmentNode.Properties["Id"] };
				}

				return document;
			});
		}

		public async Task<IEnumerable<ShipmentDocument>> GetAllAsync()
		{
			await using var session = _driver.AsyncSession();

			var query = @"
                MATCH (doc:ShipmentDocument)
                OPTIONAL MATCH (doc)-[:BELONGS_TO]->(s:Shipment)
                RETURN doc, s
            ";

			return await session.ReadTransactionAsync(async tx =>
			{
				var cursor = await tx.RunAsync(query);
				var records = await cursor.ToListAsync();

				var list = new List<ShipmentDocument>();

				foreach (var record in records)
				{
					var docNode = record["doc"].As<INode>();
					var shipmentNode = record["s"]?.As<INode>();

					var document = new ShipmentDocument
					{
						Id = (int)(long)docNode.Properties["Id"],
						ShipmentId = shipmentNode != null ? (int)(long)shipmentNode.Properties["Id"] : 0,
						DocumentType = (DocumentType)(int)(long)docNode.Properties["DocumentType"],
						FileUrl = (string)docNode.Properties["FileUrl"],
						UploadDate = DateTime.Parse((string)docNode.Properties["UploadDate"]),
						Verified = (bool)docNode.Properties["Verified"],
						VerifiedByUserId = docNode.Properties.ContainsKey("VerifiedByUserId") ? (int?)(long)docNode.Properties["VerifiedByUserId"] : null,
						ExpiryDate = docNode.Properties.ContainsKey("ExpiryDate") && docNode.Properties["ExpiryDate"] != null
							? DateTime.Parse((string)docNode.Properties["ExpiryDate"])
							: null
					};

					if (shipmentNode != null)
					{
						document.Shipment = new Shipment { Id = (int)(long)shipmentNode.Properties["Id"] };
					}

					list.Add(document);
				}

				return list;
			});
		}
	}
}
