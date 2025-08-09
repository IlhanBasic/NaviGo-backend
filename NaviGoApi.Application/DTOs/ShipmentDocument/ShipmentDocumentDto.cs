using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ShipmentDocument
{
	public class ShipmentDocumentDto
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public string DocumentType { get; set; } = null!;
		public string FileUrl { get; set; } = null!;
		public DateTime UploadDate { get; set; }
		public bool Verified { get; set; }
		public int? VerifiedByUserId { get; set; }
		public DateTime? ExpiryDate { get; set; }
	}
}
