using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Contract
{
	public class ContractCreateDto
	{
		public int ClientId { get; set; }
		public int ForwarderId { get; set; }
		public int RouteId { get; set; }
		public int RoutePriceId { get; set; }         
		public int ForwarderOfferId { get; set; }      

		public string ContractNumber { get; set; } = null!;
		public string Terms { get; set; } = null!;

		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }
	}
}
