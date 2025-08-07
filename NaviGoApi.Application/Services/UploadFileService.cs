using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class UploadFileService : IUploadFileService
	{
		private readonly BlobContainerClient _containerClient;

		public UploadFileService(IConfiguration configuration)
		{
			var connectionString = configuration["AzureBlobStorage:ConnectionString"];
			var containerName = configuration["AzureBlobStorage:ContainerName"];

			var blobServiceClient = new BlobServiceClient(connectionString);
			_containerClient = blobServiceClient.GetBlobContainerClient(containerName);
		}

		public async Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType)
		{
			var fileExtension = Path.GetExtension(originalFileName); // npr. .pdf
			var uniqueName = $"{Guid.NewGuid()}{fileExtension}";
			var blobClient = _containerClient.GetBlobClient(uniqueName);

			var headers = new BlobHttpHeaders
			{
				ContentType = contentType
			};

			await blobClient.UploadAsync(fileStream, new BlobUploadOptions
			{
				HttpHeaders = headers
			});

			return blobClient.Uri.ToString(); // npr. https://yourstorage.blob.core.windows.net/images/abc-123.pdf
		}
	}
}
