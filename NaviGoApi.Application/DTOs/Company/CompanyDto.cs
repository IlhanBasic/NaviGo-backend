using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Company
{
	public class CompanyDto
	{
		public int Id { get; set; }
		public string CompanyName { get; set; } = null!;
		public string PIB { get; set; } = null!;
		public string Address { get; set; } = null!;
		public string? LogoUrl { get; set; }
		public string ContactEmail { get; set; } = null!;
		public string? Website { get; set; }
		public string? Description { get; set; }
		public string CompanyType { get; set; } = null!;  // enum kao string
		public string CompanyStatus { get; set; } = null!; // enum kao string
		public decimal? MaxCommissionRate { get; set; }
		public decimal? SaldoAmount { get; set; }
		public DateTime CreatedAt { get; set; }
		public string? ProofFileUrl { get; set; }
	}
}
