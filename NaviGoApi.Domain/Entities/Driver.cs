using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum DriverStatus
	{
		Available = 0,
		OnRoute = 1,
		Inactive = 2
	}

	public class Driver
	{
		public int Id { get; set; }

		public int CompanyId { get; set; }
		public Company Company { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string PhoneNumber { get; set; }

		public string LicenseNumber { get; set; }

		public DateTime? LicenseExpiry { get; set; }

		// CSV string sa kategorijama dozvola, npr. "A,B1,B2,C"
		public string LicenseCategories { get; set; }

		public DateTime HireDate { get; set; }

		public DriverStatus DriverStatus { get; set; } = DriverStatus.Available;

		// Navigaciona kolekcija
		public ICollection<Shipment> Shipments { get; set; } = new List<Shipment>();
	}

}
