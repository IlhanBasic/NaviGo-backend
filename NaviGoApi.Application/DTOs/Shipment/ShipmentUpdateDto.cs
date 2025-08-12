using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Shipment
{
	public class ShipmentUpdateDto
	{
		public string? Description { get; set; }
		public ShipmentStatus Status { get; set; }
		public DateTime? ActualDeparture { get; set; }
		public DateTime? ActualArrival { get; set; }
	}
}
