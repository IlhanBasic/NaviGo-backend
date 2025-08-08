using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class Route
	{
		public int Id { get; set; }
		public int CompanyId { get; set; }
		public int StartLocationId { get; set; }
		public int EndLocationId { get; set; }
		public double DistanceKm { get; set; }
		public double EstimatedDurationHours { get; set; }
		public decimal BasePrice { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime AvailableFrom { get; set; }
		public DateTime AvailableTo { get; set; }	
		public string? GeometryEncoded { get; set; }  
		public string? GeometryJson { get; set; } 

		// Navigaciona svojstva
		public Company? Company { get; set; }
		public Location? StartLocation { get; set; }
		public Location? EndLocation { get; set; }
		public ICollection<RoutePrice>? RoutePrices { get; set; }
		public ICollection<ForwarderOffer>? ForwarderOffers { get; set; }
		public ICollection<Contract>? Contracts { get; set; }
	}

}
