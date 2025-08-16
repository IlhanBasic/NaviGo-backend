using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.RoutePrice;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.RoutePrice
{
	public class AddRoutePriceCommandHandler : IRequestHandler<AddRoutePriceCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddRoutePriceCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}
		public async Task<Unit> Handle(AddRoutePriceCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("User doesn't work in any company.");
			var routeExists = await _unitOfWork.Routes.GetByIdAsync(request.RoutePriceDto.RouteId);
			if(routeExists == null)
				throw new ValidationException($"Route with ID {request.RoutePriceDto.RouteId} does not exist.");
			var company = await _unitOfWork.Companies.GetByIdAsync(routeExists.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {routeExists.CompanyId} doesn't exists");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must be Carrier for adding route price.");
			if (company.Id != user.CompanyId.Value)
				throw new ValidationException("User doesn't work in target company.");
			var vehicleTypeExists = await _unitOfWork.VehicleTypes.GetByIdAsync(request.RoutePriceDto.VehicleTypeId);
			if(vehicleTypeExists == null)
				throw new ValidationException($"Vehicle type with ID {request.RoutePriceDto.VehicleTypeId} does not exist.");
			if (request.RoutePriceDto.PricePerKm < 0)
				throw new ValidationException("Price per km cannot be negative.");

			if (request.RoutePriceDto.MinimumPrice < 0)
				throw new ValidationException("Minimum price cannot be negative.");
			var exists = await _unitOfWork.RoutePrices.DuplicateRoutePrice(request.RoutePriceDto.RouteId, request.RoutePriceDto.VehicleTypeId);
			if (exists != null)
				throw new ValidationException("Price for this route and vehicle type already exists.");
			var entity = _mapper.Map<Domain.Entities.RoutePrice>(request.RoutePriceDto);

			await _unitOfWork.RoutePrices.AddAsync(entity);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}

	}

}
