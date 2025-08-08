using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class UserCreateDto
	{
		public string Email { get; set; }
		public string Password { get; set; }
		public string FirstName { get; set; }
		public string LastName { get; set; }
		public string PhoneNumber { get; set; }

		public UserRole UserRole { get; set; }         // enum
		public UserStatus UserStatus { get; set; }     // enum

		public bool EmailVerified { get; set; } = false;
		public int? CompanyId { get; set; }
	}
}
