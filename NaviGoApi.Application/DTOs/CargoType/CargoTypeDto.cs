using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.CargoType
{
	public class CargoTypeDto
	{
		public int Id { get; set; }
		public string TypeName { get; set; } = null!;
		public string? Description { get; set; }
		public bool RequiresSpecialEquipment { get; set; }
	}
}
