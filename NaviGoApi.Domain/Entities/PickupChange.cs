using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class PickupChange
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public int ClientId { get; set; }
		public DateTime OldTime { get; set; }
		public DateTime NewTime { get; set; }
		public int ChangeCount { get; set; }
		public decimal AdditionalFee { get; set; }

		// Navigaciona svojstva
		public Shipment? Shipment { get; set; }
		public User? Client { get; set; }
	}
}
