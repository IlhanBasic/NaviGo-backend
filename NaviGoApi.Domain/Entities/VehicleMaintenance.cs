using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum Severity
	{
		Low = 0,
		Medium = 1,
		High = 2,
		Critical = 3
	}

	public enum MaintenanceType
	{
		Regular = 0,
		Repair = 1,
		Emergency = 2
	}
	public class VehicleMaintenance
	{
		public int Id { get; set; }
		public int VehicleId { get; set; }
		public int ReportedByUserId { get; set; }
		public string Description { get; set; } = null!;
		public DateTime ReportedAt { get; set; }
		public DateTime? ResolvedAt { get; set; }
		public Severity Severity { get; set; }
		public decimal? RepairCost { get; set; }
		public MaintenanceType MaintenanceType { get; set; }

		// Navigaciona svojstva
		public Vehicle? Vehicle { get; set; }
		public User? ReportedByUser { get; set; }
	}
}
