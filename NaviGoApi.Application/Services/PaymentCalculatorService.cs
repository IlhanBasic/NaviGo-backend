using Microsoft.AspNetCore.Http;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class PaymentCalculatorService : IPaymentCalculatorService
	{
		private readonly IUnitOfWork _unitOfWork;


		public PaymentCalculatorService(IUnitOfWork unitOfWork, IHttpContextAccessor httpContextAccessor)
		{
			_unitOfWork = unitOfWork;
			
		}

		public async Task<decimal> CalculatePaymentAmountAsync(int contractId)
		{
			
			var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId)
				?? throw new ValidationException($"Contract with ID {contractId} does not exist.");
			
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
				?? throw new ValidationException($"Route with ID {contract.RouteId} not found.");
			decimal totalAmount = route.BasePrice;
			decimal penalty = await CalculatePenaltyAsync(contractId, totalAmount, contract);
			totalAmount += penalty;
			return totalAmount;
		}

		private async Task<decimal> CalculatePenaltyAsync(int contractId, decimal basePrice, Domain.Entities.Contract contract)
		{
			var penalties = await _unitOfWork.DelayPenalties.GetByContractIdAsync(contractId);
			if (penalties == null || !penalties.Any())
				return 0m;
			int totalDelayHours = penalties.Sum(p => p.DelayHours);
			decimal penaltyAmount = totalDelayHours * contract.PenaltyRatePerHour;
			decimal maxPenaltyAmount = contract.MaxPenaltyPercent / 100m * basePrice;
			return Math.Min(penaltyAmount, maxPenaltyAmount);
		}
	}
}
