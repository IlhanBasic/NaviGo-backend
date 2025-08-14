using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public LogoutCommandHandler(
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");
			var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var refreshToken = request.LogoutRequestDto.RefreshToken;
			var tokenEntity = await _unitOfWork.Users.GetRefreshTokenAsync(refreshToken);
			if (tokenEntity != null && tokenEntity.IsActive)
			{
				tokenEntity.Revoked = DateTime.UtcNow;
				tokenEntity.RevokedByIp = ipAddress;

				await _unitOfWork.SaveChangesAsync();
			}
			return Unit.Value;
		}
	}

}
