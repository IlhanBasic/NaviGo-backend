using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Driver
{
	public class DriverCreateDto
	{
		public int CompanyId { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string PhoneNumber { get; set; }

		public string LicenseNumber { get; set; }

		public DateTime? LicenseExpiry { get; set; }

		// CSV string for license categories, e.g. "A,B1,B2,C"
		public string LicenseCategories { get; set; }

		public DateTime HireDate { get; set; }

		public DriverStatus DriverStatus { get; set; } = DriverStatus.Available;
	}
}
