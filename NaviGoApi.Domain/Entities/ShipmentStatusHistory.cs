using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class ShipmentStatusHistory
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public ShipmentStatus ShipmentStatus { get; set; }

		public DateTime ChangedAt { get; set; }
		public int ChangedByUserId { get; set; }
		public string? Notes { get; set; }

		// Navigaciona svojstva
		public Shipment? Shipment { get; set; }
		public User? ChangedByUser { get; set; }
	}
}
