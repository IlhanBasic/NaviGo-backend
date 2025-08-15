using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class UpdateDriverCommandHandler : IRequestHandler<UpdateDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public UpdateDriverCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateDriverCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to update driver.");
			var existingDriver = await _unitOfWork.Drivers.GetByIdAsync(request.Id);
			if (existingDriver == null)
			{
				throw new ValidationException($"Driver with ID {request.Id} not found.");
			}
			var company = await _unitOfWork.Companies.GetByIdAsync(existingDriver.CompanyId);
			if (company == null)
				throw new ValidationException($"Company with ID {existingDriver.CompanyId} doesn't exists.");
			if (company.Id != user.CompanyId)
				throw new ValidationException("You cannot update driver to wrong company.");
			if (company.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must be Carrier to have drivers.");
			if (request.DriverDto.DriverStatus == DriverStatus.OnRoute)
			{
				var allShipments = await _unitOfWork.Shipments.GetAllAsync();
				var assignedShipments = allShipments
					.Where(s => s.DriverId == existingDriver.Id &&
								(s.Status == ShipmentStatus.Scheduled || s.Status == ShipmentStatus.InTransit))
					.ToList();

				if (!assignedShipments.Any())
				{
					throw new ValidationException("Driver cannot be set to OnRoute because no shipment/route is assigned.");
				}
			}
			_mapper.Map(request.DriverDto, existingDriver);
			await _unitOfWork.Drivers.UpdateAsync(existingDriver);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
