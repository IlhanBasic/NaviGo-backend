using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.PickupChange
{
	public class PickupChangeCreateDto
	{
		public int ShipmentId { get; set; }
		public int ClientId { get; set; }
		public DateTime OldTime { get; set; }
		public DateTime NewTime { get; set; }
		public int ChangeCount { get; set; }
		public decimal AdditionalFee { get; set; }
		public int PickupChangesStatus { get; set; }
	}
}
