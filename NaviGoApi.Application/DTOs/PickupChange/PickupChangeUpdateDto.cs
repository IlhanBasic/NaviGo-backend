using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.PickupChange
{
	public class PickupChangeUpdateDto
	{
		public DateTime OldTime { get; set; }
		public DateTime NewTime { get; set; }
		public int ChangeCount { get; set; }
		public decimal AdditionalFee { get; set; }
		public int PickupChangesStatus { get; set; }
	}
}
