using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.RoutePrice
{
	public class RoutePriceUpdateDto
	{
		public int RouteId { get; set; }
		//public int CompanyId { get; set; }
		public int VehicleTypeId { get; set; }
		public decimal PricePerKm { get; set; }
		public decimal MinimumPrice { get; set; }
	}
}
