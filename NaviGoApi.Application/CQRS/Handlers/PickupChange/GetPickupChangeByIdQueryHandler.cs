using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.PickupChange
{
	public class GetPickupChangeByIdQueryHandler : IRequestHandler<GetPickupChangeByIdQuery, PickupChangeDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetPickupChangeByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<PickupChangeDto?> Handle(GetPickupChangeByIdQuery request, CancellationToken cancellationToken)
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
			var entity = await _unitOfWork.PickupChanges.GetByIdAsync(request.Id);
			if (entity == null)
				return null;

			return _mapper.Map<PickupChangeDto>(entity);
		}
	}
}
