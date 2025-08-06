using NaviGoApi.Domain.Entities.NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum VehicleStatus
	{
		Free = 0,
		OnRoute = 1,
		InService = 2,
		Unavailable = 3
	}

	public class Vehicle
	{
		public int Id { get; set; }

		public int CompanyId { get; set; }
		public Company Company { get; set; }

		public int VehicleTypeId { get; set; }
		public VehicleType VehicleType { get; set; }

		public string RegistrationNumber { get; set; }

		public int CapacityKg { get; set; }

		public int ManufactureYear { get; set; }

		public VehicleStatus VehicleStatus { get; set; } = VehicleStatus.Free;

		public DateTime? LastInspectionDate { get; set; }

		public DateTime? InsuranceExpiry { get; set; }

		public int? CurrentLocationId { get; set; }
		public Location? CurrentLocation { get; set; }

		public bool IsAvailable { get; set; } = true;

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		// Ovo polje je CSV string sa kategorijama vozila
		public string? Categories { get; set; }

		// Navigacione kolekcije
		public ICollection<VehicleMaintenance> VehicleMaintenances { get; set; } = new List<VehicleMaintenance>();

		public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
	}

}
