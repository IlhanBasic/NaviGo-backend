using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.Services;

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
				return BadRequest("File is empty");

			// Uploaduje fajl i vraća URL
			var url = await _uploadFileService.UploadFileAsync(file.OpenReadStream(), file.FileName, file.ContentType);

			return Ok(new { Url = url });
		}
	}

}
