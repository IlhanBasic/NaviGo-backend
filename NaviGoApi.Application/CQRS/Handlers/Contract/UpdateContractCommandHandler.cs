using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Application.DTOs.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class UpdateContractCommandHandler : IRequestHandler<UpdateContractCommand, Unit>
	{
		private readonly IMapper _mapper;
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public UpdateContractCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_mapper = mapper;
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(UpdateContractCommand request, CancellationToken cancellationToken)
		{
			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");

			var existingContract = await _unitOfWork.Contracts.GetByIdAsync(request.Id)
				?? throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");
			if (currentUser.UserRole != UserRole.CompanyAdmin || currentUser.CompanyId != existingContract.ForwarderId)
				throw new ValidationException("User is not authorized to update this contract.");

			var dto = request.ContractDto;
			if (dto.PenaltyRatePerHour.HasValue && dto.PenaltyRatePerHour.Value < 0)
				throw new ValidationException("Penalty rate cannot be negative.");

			if (dto.MaxPenaltyPercent.HasValue &&
				(dto.MaxPenaltyPercent.Value < 0 || dto.MaxPenaltyPercent.Value > 100))
				throw new ValidationException("Max penalty percent must be between 0 and 100.");
			if (dto.Terms != null)
				existingContract.Terms = dto.Terms;

			if (dto.ContractStatus.HasValue)
				existingContract.ContractStatus = dto.ContractStatus.Value;

			if (dto.PenaltyRatePerHour.HasValue)
				existingContract.PenaltyRatePerHour = dto.PenaltyRatePerHour.Value;

			if (dto.MaxPenaltyPercent.HasValue)
				existingContract.MaxPenaltyPercent = dto.MaxPenaltyPercent.Value;

			await _unitOfWork.Contracts.UpdateAsync(existingContract);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
