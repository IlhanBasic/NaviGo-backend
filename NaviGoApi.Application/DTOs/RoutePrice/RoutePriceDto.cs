using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.RoutePrice
{
	public class RoutePriceDto
	{
		public int Id { get; set; }
		public int RouteId { get; set; }
		public decimal PricePerKm { get; set; }
		public decimal PricePerKg { get; set; }
		public decimal MinimumPrice { get; set; }
		public string VehicleTypeName { get; set; } = String.Empty;
		public int VehicleTypeId { get; set; }
	}
}
