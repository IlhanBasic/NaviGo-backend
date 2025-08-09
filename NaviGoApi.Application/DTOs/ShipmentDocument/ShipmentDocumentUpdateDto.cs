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
		// Možeš ostaviti Id ako ti treba za update, ili se prosleđuje odvojeno
		public DocumentType DocumentType { get; set; }
		public string FileUrl { get; set; } = null!;
		public DateTime UploadDate { get; set; }
		public bool Verified { get; set; }
		public int? VerifiedByUserId { get; set; }
		public DateTime? ExpiryDate { get; set; }
	}
}
