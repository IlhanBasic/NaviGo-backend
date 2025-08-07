using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public interface IUploadFileService
	{
		Task<string> UploadFileAsync(Stream fileStream, string originalFileName, string contentType);
	}


}
