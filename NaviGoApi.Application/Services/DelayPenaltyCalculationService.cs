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

			var delayHoursDecimal = (shipment.ActualArrival.Value - shipment.ScheduledArrival).TotalHours;
			if (delayHoursDecimal <= 0)
				return null;

			int delayHours = (int)Math.Ceiling(delayHoursDecimal);
			Console.WriteLine($"DEBUG: DelayHoursDecimal = {delayHoursDecimal}, Zaokruženo DelayHours = {delayHours}");

			//var contract = shipment.Contract ?? throw new ValidationException("Contract is not loaded for shipment.");
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId);
			if(contract==null)
				throw new ValidationException("Contract is not loaded for shipment.");
			//var payment = contract.Payment;
			var payment = (await _unitOfWork.Payments.GetByContractIdAsync(contract.Id)).FirstOrDefault();
			if (payment == null)
				throw new ValidationException("Payment is not loaded or not found for this contract.");


			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId);
			if(route ==null)
				throw new ValidationException("Route is not loaded for shipment.");

			//if (shipment.Vehicle?.VehicleTypeId == null)
			//	throw new ValidationException("Vehicle type is not defined for this shipment.");
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId);
			var vehicleTypeId = vehicle.VehicleTypeId;
			Console.WriteLine($"DEBUG: VehicleTypeId = {vehicleTypeId}");

			//var routePrice = route.RoutePrices?.FirstOrDefault(rp => rp.VehicleTypeId == vehicleTypeId)
			//	?? throw new ValidationException($"No price defined for vehicle type ID {vehicleTypeId} on this route.");
			var routePrices = await _unitOfWork.RoutePrices.GetAllAsync();
			var routePrice = routePrices.FirstOrDefault(rp => rp.VehicleTypeId == vehicleTypeId);
			var transportPrice = (decimal)route.DistanceKm * routePrice.PricePerKm;
			Console.WriteLine($"DEBUG: DistanceKm = {route.DistanceKm}, PricePerKm = {routePrice.PricePerKm}, TransportPrice = {transportPrice}");

			var penaltyRatePerHour = contract.PenaltyRatePerHour;
			var maxPenaltyPercent = contract.MaxPenaltyPercent;

			Console.WriteLine($"DEBUG: PenaltyRatePerHour = {penaltyRatePerHour}, MaxPenaltyPercent = {maxPenaltyPercent}");

			decimal penaltyAmount = transportPrice * (penaltyRatePerHour / 100m) * delayHours;
			Console.WriteLine($"DEBUG: RawPenaltyAmount = {penaltyAmount}");

			decimal maxPenaltyAmount = transportPrice * (maxPenaltyPercent / 100m);
			Console.WriteLine($"DEBUG: MaxPenaltyAmount = {maxPenaltyAmount}");

			if (penaltyAmount > maxPenaltyAmount)
			{
				Console.WriteLine("DEBUG: Penal je veći od maksimalnog - primenjujem MaxPenaltyAmount");
				penaltyAmount = maxPenaltyAmount;
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

			Console.WriteLine($"DEBUG: FinalPenaltyAmount = {penaltyAmount}");

			return await _unitOfWork.DelayPenalties.GetByShipmentIdAsync(shipment.Id);
		}

	}
}