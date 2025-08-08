using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.Services;
using System;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[ApiController]
	[Route("api/[controller]")]
	public class FileController : ControllerBase
	{
		private readonly IUploadFileService _uploadFileService;

		public FileController(IUploadFileService uploadFileService)
		{
			_uploadFileService = uploadFileService;
		}

		[HttpPost("upload")]
		public async Task<IActionResult> UploadFile(IFormFile file)
		{
			if (file == null || file.Length == 0)
				return BadRequest(new { success = false, message = "File is empty or not provided." });

			var allowedContentTypes = new[]
			{
				// Images
				"image/jpeg",
				"image/png",
				"image/gif",
				"image/bmp",
				"image/webp",

				// PDF
				"application/pdf",

				// Microsoft Office
				"application/msword",
				"application/vnd.openxmlformats-officedocument.wordprocessingml.document",
				"application/vnd.ms-excel",
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				"application/vnd.ms-powerpoint",
				"application/vnd.openxmlformats-officedocument.presentationml.presentation",

				// Text files
				"text/plain",
				"text/csv"
			};

			if (!Array.Exists(allowedContentTypes, ct => ct.Equals(file.ContentType, StringComparison.OrdinalIgnoreCase)))
				return BadRequest(new { success = false, message = "Unsupported file type." });

			try
			{
				var url = await _uploadFileService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);
				return Ok(new { success = true, message = "File uploaded successfully.", url });
			}
			catch (Exception ex)
			{
				// Možeš da loguješ grešku ovde ako imaš logger
				return StatusCode(StatusCodes.Status500InternalServerError, new { success = false, message = "File upload failed.", detail = ex.Message });
			}
		}
	}
}
