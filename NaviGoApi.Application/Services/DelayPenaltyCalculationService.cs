using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Services
{
	public class DelayPenaltyCalculationService : IDelayPenaltyCalculationService
	{
		private readonly IUnitOfWork _unitOfWork;

		public DelayPenaltyCalculationService(IUnitOfWork unitOfWork)
		{
			_unitOfWork = unitOfWork;
		}

		public async Task<DelayPenalty?> CalculateAndCreatePenaltyAsync(Shipment shipment)
		{
			if (!shipment.ActualArrival.HasValue)
				return null;

			if (!shipment.VehicleId.HasValue)
				throw new ValidationException("Shipment does not have a Vehicle assigned.");
			var delayHoursDecimal = (shipment.ActualArrival.Value - shipment.ScheduledArrival).TotalHours;
			if (delayHoursDecimal <= 0)
				return null;

			int delayHours = (int)Math.Ceiling(delayHoursDecimal);
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
						   ?? throw new ValidationException("Contract is not loaded for shipment.");
			var payment = (await _unitOfWork.Payments.GetByContractIdAsync(contract.Id)).FirstOrDefault()
						  ?? throw new ValidationException("Payment is not loaded or not found for this contract.");
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
						?? throw new ValidationException("Route is not loaded for shipment.");
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId.Value)
						  ?? throw new ValidationException("Vehicle not found.");
			var vehicleTypeId = vehicle.VehicleTypeId;
			var routePrice = (await _unitOfWork.RoutePrices.GetAllAsync())
							 .FirstOrDefault(rp => rp.RouteId == route.Id && rp.VehicleTypeId == vehicleTypeId)
							 ?? throw new ValidationException($"No price defined for vehicle type ID {vehicleTypeId} on route ID {route.Id}.");
			decimal transportPrice = (decimal)route.DistanceKm * routePrice.PricePerKm;
			decimal weightPrice = (decimal)shipment.WeightKg * routePrice.PricePerKg;
			transportPrice += weightPrice;
			transportPrice = Math.Max(transportPrice, routePrice.MinimumPrice);
			var forwarderOffer = await _unitOfWork.ForwarderOffers.GetByIdAsync(contract.ForwarderOfferId);
			var discountPercent = forwarderOffer?.DiscountRate ?? 0;
			transportPrice = transportPrice * (1 - discountPercent / 100m);
			var penaltyRatePerHour = contract.PenaltyRatePerHour;
			var maxPenaltyPercent = contract.MaxPenaltyPercent;
			decimal penaltyAmount = transportPrice * (penaltyRatePerHour / 100m) * delayHours;
			decimal maxPenaltyAmount = transportPrice * (maxPenaltyPercent / 100m);

			if (penaltyAmount > maxPenaltyAmount)
			{
				penaltyAmount = maxPenaltyAmount;
				Console.WriteLine("DEBUG: Penal je veći od maksimalnog - primenjujem MaxPenaltyAmount");
			}

			var existingPenalty = await _unitOfWork.DelayPenalties.GetByShipmentIdAsync(shipment.Id);
			if (existingPenalty != null)
			{
				existingPenalty.CalculatedAt = shipment.ActualArrival.Value;
				existingPenalty.DelayHours = delayHours;
				existingPenalty.PenaltyAmount = penaltyAmount;
				existingPenalty.DelayPenaltiesStatus = DelayPenaltyStatus.Calculated;

				await _unitOfWork.DelayPenalties.UpdateAsync(existingPenalty);
			}
			else
			{
				var delayPenalty = new DelayPenalty
				{
					ShipmentId = shipment.Id,
					CalculatedAt = shipment.ActualArrival.Value,
					DelayHours = delayHours,
					PenaltyAmount = penaltyAmount,
					DelayPenaltiesStatus = DelayPenaltyStatus.Calculated
				};

				await _unitOfWork.DelayPenalties.AddAsync(delayPenalty);
			}

			payment.PenaltyAmount = penaltyAmount;
			payment.IsRefunded = true;
			payment.RefundDate = shipment.ActualArrival.Value;

			await _unitOfWork.Payments.UpdateAsync(payment);
			await _unitOfWork.SaveChangesAsync();
			return await _unitOfWork.DelayPenalties.GetByShipmentIdAsync(shipment.Id);
		}
	}
}
