using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class ChangePasswordRequestDto
	{
		public string CurrentPassword { get; set; } 
		public string NewPassword { get; set; }
	}
}
