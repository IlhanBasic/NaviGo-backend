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

			// Izračun kašnjenja u satima
			var delayHoursDecimal = (shipment.ActualArrival.Value - shipment.ScheduledArrival).TotalHours;
			if (delayHoursDecimal <= 0)
				return null;

			int delayHours = (int)Math.Ceiling(delayHoursDecimal);
			Console.WriteLine($"DEBUG: DelayHoursDecimal = {delayHoursDecimal}, Zaokruženo DelayHours = {delayHours}");

			// Učitavanje ugovora
			var contract = await _unitOfWork.Contracts.GetByIdAsync(shipment.ContractId)
						   ?? throw new ValidationException("Contract is not loaded for shipment.");

			// Učitavanje plaćanja
			var payment = (await _unitOfWork.Payments.GetByContractIdAsync(contract.Id)).FirstOrDefault()
						  ?? throw new ValidationException("Payment is not loaded or not found for this contract.");

			// Učitavanje rute
			var route = await _unitOfWork.Routes.GetByIdAsync(contract.RouteId)
						?? throw new ValidationException("Route is not loaded for shipment.");

			// Učitavanje vozila i tipa
			var vehicle = await _unitOfWork.Vehicles.GetByIdAsync(shipment.VehicleId)
						  ?? throw new ValidationException("Vehicle not found.");
			var vehicleTypeId = vehicle.VehicleTypeId;

			// Učitavanje cene po tipu vozila
			var routePrices = await _unitOfWork.RoutePrices.GetAllAsync();
			var routePrice = routePrices.FirstOrDefault(rp => rp.VehicleTypeId == vehicleTypeId)
							 ?? throw new ValidationException($"No price defined for vehicle type ID {vehicleTypeId} on this route.");

			// Osnovna cena transporta
			decimal transportPrice = (decimal)route.DistanceKm * routePrice.PricePerKm;

			// Učitavanje eventualnog popusta od špediter ponude
			var forwarderOffer = (await _unitOfWork.ForwarderOffers.GetByRouteIdAsync(route.Id))
								 .FirstOrDefault(f => f.ForwarderOfferStatus == ForwarderOfferStatus.Accepted);
			var discountPercent = forwarderOffer?.DiscountRate ?? 0;
			transportPrice = transportPrice * (1 - discountPercent / 100m);

			Console.WriteLine($"DEBUG: DistanceKm = {route.DistanceKm}, PricePerKm = {routePrice.PricePerKm}, Discount = {discountPercent}%, TransportPriceAfterDiscount = {transportPrice}");

			// Penal po satu i maksimalni procenat
			var penaltyRatePerHour = contract.PenaltyRatePerHour;
			var maxPenaltyPercent = contract.MaxPenaltyPercent;

			decimal penaltyAmount = transportPrice * (penaltyRatePerHour / 100m) * delayHours;
			decimal maxPenaltyAmount = transportPrice * (maxPenaltyPercent / 100m);

			if (penaltyAmount > maxPenaltyAmount)
			{
				penaltyAmount = maxPenaltyAmount;
				Console.WriteLine("DEBUG: Penal je veći od maksimalnog - primenjujem MaxPenaltyAmount");
			}

			// Provera da li već postoji penal za pošiljku
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

			// Ažuriranje plaćanja
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
