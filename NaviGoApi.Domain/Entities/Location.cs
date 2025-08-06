using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	namespace NaviGoApi.Domain.Entities
	{
		public class Location
		{
			public int Id { get; set; }
			public string City { get; set; } = null!;
			public string Country { get; set; } = null!;
			public string ZIP { get; set; } = null!;
			public double Latitude { get; set; }
			public double Longitude { get; set; }
			public string FullAddress { get; set; } = null!;

			// Navigaciona svojstva
			public ICollection<Route>? RoutesStart { get; set; }
			public ICollection<Route>? RoutesEnd { get; set; }
			public ICollection<Vehicle>? VehiclesCurrentLocation { get; set; }
		}
	}

}
