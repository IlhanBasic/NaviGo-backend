using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Shipment
{
	public class ShipmentCreateDto
	{
		public int ContractId { get; set; }
		public int VehicleId { get; set; }
		public int DriverId { get; set; }
		public int CargoTypeId { get; set; }
		public double WeightKg { get; set; }
		public int Priority { get; set; }
		public string? Description { get; set; }
		public ShipmentStatus Status { get; set; }

		public DateTime ScheduledDeparture { get; set; }
		public DateTime ScheduledArrival { get; set; }
		public DateTime? ActualDeparture { get; set; }
		public DateTime? ActualArrival { get; set; }
		public int? DelayHours { get; set; }
		public decimal? DelayPenaltyAmount { get; set; }
		public DateTime? PenaltyCalculatedAt { get; set; }
	}
}
