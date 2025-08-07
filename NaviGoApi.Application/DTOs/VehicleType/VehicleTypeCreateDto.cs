using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.VehicleType
{
	public class VehicleTypeCreateDto
	{
		public string TypeName { get; set; } = null!;
		public string? Description { get; set; }
		public bool RequiresSpecialLicense { get; set; }
	}
}
