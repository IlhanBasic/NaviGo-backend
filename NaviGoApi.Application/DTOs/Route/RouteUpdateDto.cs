using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Route
{
	public class RouteUpdateDto
	{
		public int StartLocationId { get; set; }
		public int EndLocationId { get; set; }
		public decimal BasePrice { get; set; }
		public bool IsActive { get; set; }
		public DateTime AvailableFrom { get; set; }
		public DateTime AvailableTo { get; set; }
	}
}
