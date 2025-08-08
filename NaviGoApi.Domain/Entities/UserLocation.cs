using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class UserLocation
	{
		public int Id { get; set; }
		public int UserId { get; set; }
		public string IpAddress { get; set; } = null!;
		public string Region { get; set; } = null!;
		public DateTime AccessTime { get; set; }

		public User User { get; set; } = null!;
	}

}
