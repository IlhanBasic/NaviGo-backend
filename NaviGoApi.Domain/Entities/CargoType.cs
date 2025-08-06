using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class CargoType
	{
		public int Id { get; set; }
		public string TypeName { get; set; } = null!;
		public string? Description { get; set; }
		public int HazardLevel { get; set; }
		public bool RequiresSpecialEquipment { get; set; }

		// Navigaciona svojstva
		public ICollection<Shipment>? Shipments { get; set; }
	}
}
