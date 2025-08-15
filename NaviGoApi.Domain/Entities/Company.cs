
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NaviGoApi.Domain.Entities
{
	public enum CompanyType
	{
		Client = 1,
		Forwarder = 2,
		Carrier = 3
	}

	public enum CompanyStatus
	{
		Pending = 0,
		Approved = 1,
		Rejected = 2
	}

	public class Company
	{
		public int Id { get; set; }

		public string CompanyName { get; set; }

		public string PIB { get; set; }

		public string Address { get; set; }

		public string? LogoUrl { get; set; }

		public string ContactEmail { get; set; }

		public string? Website { get; set; }

		public string? Description { get; set; }

		public CompanyType CompanyType { get; set; }

		public CompanyStatus CompanyStatus { get; set; } = CompanyStatus.Pending;

		public decimal? MaxCommissionRate { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
		public string? ProofFileUrl { get; set; }

		// Navigation properties
		public ICollection<User> Users { get; set; } = new List<User>();

		public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();

		public ICollection<Driver> Drivers { get; set; } = new List<Driver>();
		public ICollection<Route> Routes { get; set; } = new List<Route>();

		public ICollection<ForwarderOffer> ForwarderOffers { get; set; } = new List<ForwarderOffer>();
	}

}
