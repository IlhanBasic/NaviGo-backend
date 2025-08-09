using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.DelayPenalty
{
	public class DelayPenaltyDto
	{
		public int Id { get; set; }
		public int ShipmentId { get; set; }
		public DateTime CalculatedAt { get; set; }
		public int DelayHours { get; set; }
		public decimal PenaltyAmount { get; set; }
		public string DelayPenaltiesStatus { get; set; } =String.Empty;
	}
}
