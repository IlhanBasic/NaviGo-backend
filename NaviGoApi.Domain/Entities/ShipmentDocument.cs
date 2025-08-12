using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum DocumentType
	{
		BillOfLading = 0,
		Invoice = 1,
		PackingList = 2,
		CustomsDeclaration = 3,
		InsuranceCertificate = 4,
		Other = 99
	}

	public class ShipmentDocument
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public DocumentType DocumentType { get; set; }

		public string FileUrl { get; set; } = null!;
		public DateTime UploadDate { get; set; }
		public bool Verified { get; set; }
		public int? VerifiedByUserId { get; set; }

		// Navigaciona svojstva
		public Shipment? Shipment { get; set; }
		public User? VerifiedByUser { get; set; }
	}
}
