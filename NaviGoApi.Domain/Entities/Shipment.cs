using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum ShipmentStatus
	{
		Scheduled = 0,  // Planirano, još nije krenulo
		InTransit = 1,  // U transportu, u toku dostave
		Delivered = 2,  // Dostavljeno
		Delayed = 3,    // Kašnjenje u isporuci
		Cancelled = 4   // Otkazano
	}
	public class Shipment
	{
		public int Id { get; set; }
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
		public DateTime CreatedAt { get; set; }
		//public int? DelayHours { get; set; }
		//public decimal? DelayPenaltyAmount { get; set; }
		//public DateTime? PenaltyCalculatedAt { get; set; }

		// Navigaciona svojstva
		public Contract? Contract { get; set; }
		public Vehicle? Vehicle { get; set; }
		public Driver? Driver { get; set; }
		public CargoType? CargoType { get; set; }
		public DelayPenalty? DelayPenalty { get; set; }
		public PickupChange? PickupChange { get; set; }
		public ICollection<ShipmentDocument>? ShipmentDocuments { get; set; }
		public ICollection<ShipmentStatusHistory>? ShipmentStatusHistories { get; set; }
	}
}
