﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.DTOs.User
{
	public class UserLoginDto
	{
		public string Email { get; set; } = null!;
		public string Password { get; set; } = null!;
	}

}
