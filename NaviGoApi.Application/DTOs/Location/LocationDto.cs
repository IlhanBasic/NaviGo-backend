using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.Location
{
	public class LocationDto
	{
		public int Id { get; set; }
		public string City { get; set; } = null!;
		public string Country { get; set; } = null!;
		public string ZIP { get; set; } = null!;
		public double Latitude { get; set; }
		public double Longitude { get; set; }
		public string FullAddress { get; set; } = null!;
	}
}
