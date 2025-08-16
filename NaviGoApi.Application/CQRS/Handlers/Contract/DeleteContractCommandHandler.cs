using MediatR;
using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.CQRS.Commands.Contract;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Threading;
using System.Threading.Tasks;

namespace NaviGoApi.Application.CQRS.Handlers.Contract
{
	public class DeleteContractCommandHandler : IRequestHandler<DeleteContractCommand, Unit>
	{
		private readonly IUnitOfWork _unitOfWork;
		private readonly IHttpContextAccessor _httpContextAccessor;

		public DeleteContractCommandHandler(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			_httpContextAccessor = httpContextAccessor;
		}

		public async Task<Unit> Handle(DeleteContractCommand request, CancellationToken cancellationToken)
		{
			var contract = await _unitOfWork.Contracts.GetByIdAsync(request.Id);
			if (contract == null)
				throw new KeyNotFoundException($"Contract with ID {request.Id} not found.");

			if (_httpContextAccessor.HttpContext == null)
				throw new ValidationException("HttpContext is null.");

			var userEmail = _httpContextAccessor.HttpContext.User
				.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;

			if (string.IsNullOrEmpty(userEmail))
				throw new ValidationException("User email not found in token.");

			var currentUser = await _unitOfWork.Users.GetByEmailAsync(userEmail)
				?? throw new ValidationException("User not found.");
			if (currentUser.UserRole != UserRole.CompanyAdmin || currentUser.CompanyId != contract.ForwarderId)
			{
				throw new ValidationException("User is not authorized to delete this contract.");
			}
			await _unitOfWork.Contracts.DeleteAsync(contract);
			await _unitOfWork.SaveChangesAsync();

			return Unit.Value;
		}
	}
}
