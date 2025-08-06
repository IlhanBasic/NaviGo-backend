using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Commands.User
{
	public record GoogleAuthenticateCommand(string IdToken) : IRequest<(string accessToken, string refreshToken)?>;
}
