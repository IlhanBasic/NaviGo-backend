using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Common.DTOs
{
	public class CompanySearchDto
	{
		public string? Pib { get; set; }
		public string? CompanyName { get; set; }
		public string? SortBy { get; set; } = "Id";
		public string SortDirection { get; set; } = "asc";
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
