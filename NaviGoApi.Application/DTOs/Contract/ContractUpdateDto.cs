using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Contract
{
	public class ContractUpdateDto
	{
		public string? Terms { get; set; }
		public ContractStatus? ContractStatus { get; set; }
		public decimal? PenaltyRatePerHour { get; set; }
		public decimal? MaxPenaltyPercent { get; set; }
	}
}
