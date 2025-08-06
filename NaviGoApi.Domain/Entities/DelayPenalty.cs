using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum DelayPenaltyStatus
	{
		Pending = 0,     
		Calculated = 1, 
		Waived = 2      
	}

	public class DelayPenalty
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public DateTime CalculatedAt { get; set; }
		public int DelayHours { get; set; }
		public decimal PenaltyAmount { get; set; }
		public DelayPenaltyStatus DelayPenaltiesStatus { get; set; }

		// Navigaciona svojstva
		public Shipment? Shipment { get; set; }
	}
}
