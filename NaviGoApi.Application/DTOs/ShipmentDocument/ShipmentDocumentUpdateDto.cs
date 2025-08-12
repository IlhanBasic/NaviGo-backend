using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ShipmentDocument
{
	public class ShipmentDocumentUpdateDto
	{
		public DocumentType DocumentType { get; set; }
		public string FileUrl { get; set; } = null!;
	}
}
