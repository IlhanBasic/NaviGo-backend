using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Vehicle
{
	public class VehicleCreateDto
	{
		public int CompanyId { get; set; }

		public int VehicleTypeId { get; set; }

		public string RegistrationNumber { get; set; } = null!;

		public int CapacityKg { get; set; }

		public int ManufactureYear { get; set; }

		public VehicleStatus VehicleStatus { get; set; } = VehicleStatus.Free;

		public DateTime? LastInspectionDate { get; set; }

		public DateTime? InsuranceExpiry { get; set; }

		public int? CurrentLocationId { get; set; }

		public bool IsAvailable { get; set; } = true;

		public string? Categories { get; set; }
	}
}
