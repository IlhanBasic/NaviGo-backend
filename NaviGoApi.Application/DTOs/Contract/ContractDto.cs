using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Contract
{
	public class ContractDto
	{
		public int Id { get; set; }

		public int ClientId { get; set; }
		public string ClientFullName { get; set; }

		public int ForwarderId { get; set; }
		public string ForwarderCompanyName { get; set; }

		public int RouteId { get; set; }

		public string ContractNumber { get; set; }
		public DateTime ContractDate { get; set; }
		public string Terms { get; set; }

		public string ContractStatus { get; set; }
		public decimal PenaltyRatePerHour { get; set; }
		public decimal MaxPenaltyPercent { get; set; }

		public DateTime ValidUntil { get; set; }
		public DateTime? SignedDate { get; set; }
	}
}
