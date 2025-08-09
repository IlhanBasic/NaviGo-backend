using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.ShipmentStatusHistory
{
	public class ShipmentStatusHistoryUpdateDto
	{
		public ShipmentStatus ShipmentStatus { get; set; }
		public DateTime ChangedAt { get; set; }
		public int ChangedByUserId { get; set; }
		public string? Notes { get; set; }
	}
}
