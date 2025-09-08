using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class RoutePriceUpdateHandler : IRequestHandler<UpdateRoutePriceCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public RoutePriceUpdateHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateRoutePriceCommand request, CancellationToken cancellationToken)
		{
			var httpContext = _httpContextAccessor.HttpContext
				?? throw new InvalidOperationException("HttpContext is not available.");

			var userEmail = httpContext.User.FindFirst(ClaimTypes.Email)?.Value;
			if (string.IsNullOrWhiteSpace(userEmail))
				throw new ValidationException("User email not found in authentication token.");
			var user = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException($"User with email '{userEmail}' not found.");
			if (user.UserStatus != UserStatus.Active)
				throw new ValidationException("Your account is not activated.");
			if (user.UserRole != UserRole.CompanyAdmin)
				throw new ValidationException("You are not allowed to add route price.");
			if (user.CompanyId == null)
				throw new ValidationException("User is not associated with any company.");
			var company = await _unitOfWork.Companies.GetByIdAsync(user.CompanyId.Value);
			if (company == null)
				throw new ValidationException("User's company not found.");

			if (company.CompanyType != Domain.Entities.CompanyType.Carrier)
				throw new ValidationException("Company is not a carrier.");

			var entity = await _unitOfWork.RoutePrices.GetByIdAsync(request.Id);
			if (entity == null)
				throw new ValidationException($"RoutePrice with ID {request.Id} does not exist.");

			var route = await _unitOfWork.Routes.GetByIdAsync(entity.RouteId);
			if (route == null)
				throw new ValidationException("Associated route not found.");

			if (route.CompanyId != user.CompanyId)
				throw new ValidationException("You cannot modify prices for routes not owned by your company.");
			if (request.RoutePriceDto.PricePerKm < 0)
				throw new ValidationException("Price per km cannot be negative.");

			if (request.RoutePriceDto.PricePerKg < 0)
				throw new ValidationException("Price per kg cannot be negative.");

			if (request.RoutePriceDto.MinimumPrice < 0)
				throw new ValidationException("Minimum price cannot be negative.");
			var exists = await _unitOfWork.RoutePrices.DuplicateRoutePrice(request.RoutePriceDto.RouteId, request.RoutePriceDto.VehicleTypeId);
			if (exists != null && exists.Id != request.Id)
				throw new ValidationException("Price for this route and vehicle type already exists.");
			_mapper.Map(request.RoutePriceDto, entity);
			await _unitOfWork.RoutePrices.UpdateAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}

}
