using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.VehicleMaintenance
{
	public class VehicleMaintenanceDto
	{
		public int Id { get; set; }
		public int VehicleId { get; set; }
		public int ReportedByUserId { get; set; }
		public string Description { get; set; } = null!;
		public DateTime ReportedAt { get; set; }
		public DateTime? ResolvedAt { get; set; }
		public string Severity { get; set; } = null!;
		public string MaintenanceType { get; set; } = null!;
		public decimal? RepairCost { get; set; }
		public string? ReportedByUserEmail { get; set; }
	}

}
