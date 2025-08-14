using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class UserActivationDto
	{
		public UserStatus? UserStatus { get; set; }
		public UserRole? UserRole { get; set; }
	}
}
