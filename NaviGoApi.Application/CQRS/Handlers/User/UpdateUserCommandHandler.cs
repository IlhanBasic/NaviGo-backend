using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		private readonly IMapper _mapper;

		public UpdateUserCommandHandler(
			IUnitOfWork unitOfWork,
			IHttpContextAccessor httpContextAccessor,
			IMapper mapper)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
			_mapper = mapper;
		}

		public async Task<Unit> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");

			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");

			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");
			if (user.Id != request.Id)
				throw new ValidationException("You are not allowed to update another user's profile.");
			_mapper.Map(request.UserDto, user);
			await _unitOfWork.Users.UpdateAsync(user);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
