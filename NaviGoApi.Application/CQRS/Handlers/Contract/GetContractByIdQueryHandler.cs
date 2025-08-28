using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	internal class GetContractByIdQueryHandler : IRequestHandler<GetContractByIdQuery, ContractDto?>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetContractByIdQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<ContractDto?> Handle(GetContractByIdQuery request, CancellationToken cancellationToken)
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
			//if (user.UserRole != UserRole.CompanyAdmin && user.UserRole != UserRole.SuperAdmin)
			//	throw new ValidationException("User is not authorized to view this contract.");

			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);
			if (contract == null)
				return null;
			if (user.UserRole == UserRole.CompanyAdmin && contract.ForwarderId != user.CompanyId)
				throw new ValidationException("User is not authorized to view this contract.");

			return _mapper.Map<ContractDto>(contract);
		}
	}
}
