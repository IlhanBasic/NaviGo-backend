using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Vehicle
{
	public class VehicleUpdateDto
	{
		public int CompanyId { get; set; }
		public string Brand { get; set; }
		public string Model { get; set; }
		public int EngineCapacityCc { get; set; }
		public string? VehiclePicture {  get; set; }
		public int VehicleTypeId { get; set; }

		public string RegistrationNumber { get; set; } = null!;

		public int CapacityKg { get; set; }

		public int ManufactureYear { get; set; }

		public VehicleStatus VehicleStatus { get; set; }

		public DateTime? LastInspectionDate { get; set; }

		public DateTime? InsuranceExpiry { get; set; }

		public int? CurrentLocationId { get; set; }

		public string? Categories { get; set; }
	}
}
