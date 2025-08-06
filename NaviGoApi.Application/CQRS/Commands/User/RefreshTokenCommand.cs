using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public record RefreshTokenCommand(string Token, string IpAddress) : IRequest<(string accessToken, string refreshToken)?>;
}
