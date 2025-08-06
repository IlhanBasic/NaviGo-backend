using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class VehicleType
	{
		public int Id { get; set; }
		public string TypeName { get; set; } = null!;
		public string? Description { get; set; }
		public bool RequiresSpecialLicense { get; set; }

		// Navigaciona svojstva
		public ICollection<Vehicle>? Vehicles { get; set; }
		public ICollection<RoutePrice>? RoutePrices { get; set; }
	}
}
