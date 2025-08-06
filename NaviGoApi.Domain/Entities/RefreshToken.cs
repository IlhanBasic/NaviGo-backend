using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public class RefreshToken
	{
		public int Id { get; set; }

		public int UserId { get; set; }
		public User User { get; set; }

		public string Token { get; set; }

		public DateTime Expires { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? Revoked { get; set; }

		public bool IsActive { get; set; } = true;
	}

}
