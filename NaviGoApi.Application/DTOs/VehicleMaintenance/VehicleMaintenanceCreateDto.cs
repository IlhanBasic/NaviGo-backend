using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.VehicleMaintenance
{
	public class VehicleMaintenanceCreateDto
	{
		public int VehicleId { get; set; }
		public string Description { get; set; } = null!;
		public Severity Severity { get; set; }
		public MaintenanceType MaintenanceType { get; set; }
		public decimal? RepairCost { get; set; }
	}

}
