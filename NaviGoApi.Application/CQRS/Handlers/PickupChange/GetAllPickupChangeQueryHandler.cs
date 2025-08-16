using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class GetAllPickupChangesQueryHandler : IRequestHandler<GetAllPickupChangesQuery, IEnumerable<PickupChangeDto>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetAllPickupChangesQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<PickupChangeDto>> Handle(GetAllPickupChangesQuery request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");
			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("User must be activated.");
			var entities = await _unitOfWork.PickupChanges.GetAllAsync();
			return _mapper.Map<IEnumerable<PickupChangeDto>>(entities);
		}
	}
}
