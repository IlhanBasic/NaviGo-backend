using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Company
{
	public class CompanyUpdateDto
	{
		public string CompanyName { get; set; } = null!;
		public string Address { get; set; } = null!;
		public string? LogoUrl { get; set; }
		public string ContactEmail { get; set; } = null!;
		public string? Website { get; set; }
		public string? Description { get; set; }
		public decimal? MaxCommissionRate { get; set; }
		public string? ProofFileUrl { get; set; }
	}
}
