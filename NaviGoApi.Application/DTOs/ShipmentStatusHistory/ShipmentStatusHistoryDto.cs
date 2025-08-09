using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ShipmentStatusHistory
{
	public class ShipmentStatusHistoryDto
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public string ShipmentStatus { get; set; } = null!;
		public DateTime ChangedAt { get; set; }
		public int ChangedByUserId { get; set; }
		public string? Notes { get; set; }
	}
}
