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
		public int ClientId { get; set; }        // FK na User (klijent)
		public int ForwarderId { get; set; }     // FK na Company (špediter)
		public int RouteId { get; set; }         // FK na Route

		public string ContractNumber { get; set; } = null!;
		public DateTime ContractDate { get; set; }
		public string Terms { get; set; } = null!;

		public ContractStatus ContractStatus { get; set; }
		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }

		public DateTime ValidUntil { get; set; }
		public DateTime? SignedDate { get; set; }
	}
}
