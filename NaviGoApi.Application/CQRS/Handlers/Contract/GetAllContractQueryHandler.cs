using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Queries.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class GetAllContractQueryHandler : IRequestHandler<GetAllContractQuery, IEnumerable<ContractDto?>>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public GetAllContractQueryHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<IEnumerable<ContractDto?>> Handle(GetAllContractQuery request, CancellationToken cancellationToken)
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
			if (user.UserRole != UserRole.CompanyAdmin && user.UserRole != UserRole.SuperAdmin)
				throw new ValidationException("User is not authorized to view contracts.");

			var contracts = await _unitOfWork.Contracts.GetAllAsync();
			if (user.UserRole == UserRole.CompanyAdmin)
			{
				contracts = contracts.Where(c => c.ForwarderId == user.CompanyId).ToList();
			}

			return _mapper.Map<IEnumerable<ContractDto>>(contracts);
		}
	}
}
