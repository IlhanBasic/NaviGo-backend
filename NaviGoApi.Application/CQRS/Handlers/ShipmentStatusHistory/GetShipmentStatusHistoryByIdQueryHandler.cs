using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.ShipmentStatusHistory
{
	public class GetShipmentStatusHistoryByIdQueryHandler : IRequestHandler<GetShipmentStatusHistoryByIdQuery, ShipmentStatusHistoryDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public GetShipmentStatusHistoryByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork,IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ShipmentStatusHistoryDto?> Handle(GetShipmentStatusHistoryByIdQuery request, CancellationToken cancellationToken)
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
				throw new ValidationException("Your account is not activated.");

			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("Only Company Admins can view shipment status history.");

			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");

			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value)
				?? throw new ValidationException("Company not found.");
			var entity = await _unitOfWork.ShipmentStatusHistories.GetByIdAsync(request.Id);
			if (entity == null)
				return null;

			return _mapper.Map<ShipmentStatusHistoryDto>(entity);
		}
	}
}
