using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Shipment
{
	public class ClientShipmentCreateDto
	{
		public int CargoTypeId { get; set; }
		public double WeightKg { get; set; }
		public int Priority { get; set; }
		public string? Description { get; set; }

		public DateTime ScheduledDeparture { get; set; }
		public DateTime ScheduledArrival { get; set; }
	}
}
