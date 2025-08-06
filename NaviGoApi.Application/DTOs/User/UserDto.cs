using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class UserDto
	{
		public int Id { get; set; }
		public string Email { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhoneNumber { get; set; }

		public string UserRole { get; set; }      // enum kao string
		public string UserStatus { get; set; }    // enum kao string

		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime? LastLogin { get; set; }
		public bool EmailVerified { get; set; }
		public int? CompanyId { get; set; }
	}
}
