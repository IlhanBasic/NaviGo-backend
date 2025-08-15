using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Driver;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Driver
{
	public class AddDriverCommandHandler : IRequestHandler<AddDriverCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IMapper _mapper;
		private readonly IHttpContextAccessor _httpContextAccessor;
		public AddDriverCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_mapper = mapper;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(AddDriverCommand request, CancellationToken cancellationToken)
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
				throw new ValidationException("You are not allowed to add vehicle.");
			var companyExists = await _unitOfWork.Companies.GetByIdAsync(request.DriverDto.CompanyId);
			if (companyExists == null)
				throw new ValidationException($"Company with ID {request.DriverDto.CompanyId} does not exist.");
			if (user.CompanyId != companyExists.Id)
				throw new ValidationException("You cannot add driver to wrong company.");
			if (companyExists.CompanyType != CompanyType.Carrier)
				throw new ValidationException("Company must be Carrier to have drivers.");
			var driver = _mapper.Map<Domain.Entities.Driver>(request.DriverDto);
			await _unitOfWork.Drivers.AddAsync(driver);
			await _unitOfWork.SaveChangesAsync();
			return Unit.Value;
		}
	}
}
