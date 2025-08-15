using Microsoft.AspNetCore.Http;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class PaymentCalculatorService : IPaymentCalculatorService
	{
		private readonly IUnitOfWork _unitOfWork;

		public PaymentCalculatorService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<decimal> CalculatePaymentAmountAsync(int contractId)
		{
			var contract = await _unitOfWork.Contracts.GetByIdAsync(contractId)
				?? throw new ValidationException($"Contract with ID {contractId} does not exist.");

			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
				?? throw new ValidationException($"Route with ID {contract.RouteId} not found.");

			var shipment = (await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id)).FirstOrDefault();
			if (shipment == null)
				throw new ValidationException($"No shipment found for contract ID {contract.Id}.");

			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId);
			if (vehicle == null)
				throw new ValidationException($"Vehicle with ID {shipment.VehicleId} not found.");

			var routePrices = await _unitOfWork.RoutePrices.GetAllAsync();
			var routePrice = routePrices.FirstOrDefault(rp => rp.RouteId == route.Id && rp.VehicleTypeId == vehicle.VehicleTypeId)
				?? throw new ValidationException($"No price defined for vehicle type ID {vehicle.VehicleTypeId} on this route.");

			decimal transportPrice = (decimal)route.DistanceKm * routePrice.PricePerKm;

			decimal penalty = await CalculatePenaltyAsync(contractId, transportPrice, contract);

			return transportPrice + penalty;
		}

		private async Task<decimal> CalculatePenaltyAsync(int contractId, decimal transportPrice, Domain.Entities.Contract contract)
		{
			var penalties = await _unitOfWork.DelayPenalties.GetByContractIdAsync(contractId);
			if (penalties == null || !penalties.Any())
				return 0m;

			int totalDelayHours = penalties.Sum(p => p.DelayHours);
			decimal penaltyAmount = transportPrice * (contract.PenaltyRatePerHour / 100m) * totalDelayHours;
			decimal maxPenaltyAmount = transportPrice * (contract.MaxPenaltyPercent / 100m);

			return Math.Min(penaltyAmount, maxPenaltyAmount);
		}
	}
}
