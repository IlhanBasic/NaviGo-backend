using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class ResetPasswordRequestDto
	{
		public string Token { get; set; }    
		public string NewPassword { get; set; }
	}
}
