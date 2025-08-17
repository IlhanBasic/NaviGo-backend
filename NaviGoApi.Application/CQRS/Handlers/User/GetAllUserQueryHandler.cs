using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.User
{
	using AutoMapper;
	using global::NaviGoApi.Application.CQRS.Queries.User;
	using global::NaviGoApi.Application.DTOs.User;
	using global::NaviGoApi.Domain.Entities;
	using global::NaviGoApi.Domain.Interfaces;
	using MediatR;
	using Microsoft.AspNetCore.Http;
	using System.Collections.Generic;
	using System.ComponentModel.DataAnnotations;
	using System.Security.Claims;
	using System.Threading;
	using System.Threading.Tasks;

	namespace NaviGoApi.Application.CQRS.Handlers.User
	{
		public class GetAllUserQueryHandler : IRequestHandler<GetAllUserQuery, IEnumerable<UserDto>>
		{
			private readonly IUnitOfWork _unitOfWork;
			private readonly IMapper _mapper;
			private readonly IHttpContextAccessor _httpContextAccessor;
			public GetAllUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
			{
				_unitOfWork = unitOfWork;
				_mapper = mapper;
				_httpContextAccessor = httpContextAccessor;
			}
			public async Task<IEnumerable<UserDto>> Handle(GetAllUserQuery request, CancellationToken cancellationToken)
			{
				var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");
				var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
				if (string.IsNullOrWhiteSpace(userEmail))
					throw new ValidationException("User email not found in authentication token.");
				var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
					?? throw new ValidationException($"User with email '{userEmail}' not found.");
				if (currentUser.UserStatus != UserStatus.Active)
					throw new ValidationException("Your account is not activated.");
				//if (currentUser.UserRole != UserRole.SuperAdmin)
				//	throw new ValidationException("Only SuperAdmin have right to view all users.");
				var users = await _unitOfWork.Users.GetAllAsync();
				var userDtos = _mapper.Map<IEnumerable<UserDto>>(users);
				return userDtos;
			}
		}
	}

}
