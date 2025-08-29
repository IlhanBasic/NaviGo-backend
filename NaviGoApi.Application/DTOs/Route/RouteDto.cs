using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Route
{
	public class RouteDto
	{
		public int Id { get; set; }
		public int CompanyId { get; set; }
		public string CompanyName { get; set; } = string.Empty;
		public int StartLocationId { get; set; }
		public string StartLocationName { get; set; }= string.Empty;
		public int EndLocationId { get; set; }
		public string EndLocationName { get; set; } = string.Empty;
		public double DistanceKm { get; set; }
		public double EstimatedDurationHours { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime AvailableFrom { get; set; }
		public DateTime AvailableTo { get; set; }
		public string? GeometryEncoded { get; set; }
	}
}
