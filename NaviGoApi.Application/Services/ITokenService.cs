using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public interface ITokenService
	{
		Task<string> GenerateJwtToken(User user);
		RefreshToken GenerateRefreshToken(string ipAddress, int userId);
	}
}
