using Microsoft.AspNetCore.Http;
using NaviGoApi.Domain.Entities;
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

			var shipments = await _unitOfWork.Shipments.GetByContractIdAsync(contract.Id);
			if (shipments == null || !shipments.Any())
				throw new ValidationException($"No shipment found for contract ID {contract.Id}.");

			decimal totalWeightPrice = 0m;
			decimal transportPrice = 0m;

			foreach (var shipment in shipments)
			{
				if (!shipment.VehicleId.HasValue)
					throw new ValidationException($"Shipment with ID {shipment.Id} does not have a Vehicle assigned.");

				var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId.Value)
							  ?? throw new ValidationException($"Vehicle with ID {shipment.VehicleId} not found.");

				var routePrices = await _unitOfWork.RoutePrices.GetAllAsync();
				var routePrice = routePrices.FirstOrDefault(rp => rp.RouteId == route.Id && rp.VehicleTypeId == vehicle.VehicleTypeId)
					?? throw new ValidationException($"No price defined for vehicle type ID {vehicle.VehicleTypeId} on this route.");

				var shipmentTransportPrice = (decimal)route.DistanceKm * routePrice.PricePerKm;
				if (shipment.Priority == 1)
					shipmentTransportPrice *= 1.2m;
				shipmentTransportPrice = Math.Max(shipmentTransportPrice, routePrice.MinimumPrice);
				var shipmentWeightPrice = (decimal)shipment.WeightKg * routePrice.PricePerKg;

				transportPrice += shipmentTransportPrice;
				totalWeightPrice += shipmentWeightPrice;
			}

			decimal totalPriceBeforePenalty = transportPrice + totalWeightPrice;

			decimal penalty = await CalculatePenaltyAsync(contractId, totalPriceBeforePenalty, contract);

			return Math.Max(0, totalPriceBeforePenalty - penalty);
		}

		private async Task<decimal> CalculatePenaltyAsync(int contractId, decimal totalPrice, Contract contract)
		{
			var penalties = await _unitOfWork.DelayPenalties.GetByContractIdAsync(contractId);
			if (penalties == null || !penalties.Any())
				return 0m;

			int totalDelayHours = penalties.Sum(p => p.DelayHours);
			decimal penaltyAmount = totalPrice * (contract.PenaltyRatePerHour / 100m) * totalDelayHours;
			decimal maxPenaltyAmount = totalPrice * (contract.MaxPenaltyPercent / 100m);

			return Math.Min(penaltyAmount, maxPenaltyAmount);
		}
	}
}
