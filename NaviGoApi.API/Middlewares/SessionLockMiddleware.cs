using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using NaviGoApi.Application.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace NaviGoApi.API.Middlewares
{
	public class SessionLockMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly IGeoLocationService _geoLocationService;
		private readonly IUnitOfWork _unitOfWork;
		private readonly ILogger<SessionLockMiddleware> _logger;

		private static readonly TimeSpan LocationCheckInterval = TimeSpan.FromMinutes(5);

		public SessionLockMiddleware(RequestDelegate next,
			IGeoLocationService geoLocationService,
			IUnitOfWork unitOfWork,
			ILogger<SessionLockMiddleware> logger)
		{
			_next = next;
			_geoLocationService = geoLocationService;
			_unitOfWork = unitOfWork;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
			var userIdClaim = context.User.FindFirst("id")?.Value;

			if (!int.TryParse(userIdClaim, out int userId))
			{
				// Nema prijavljenog korisnika - nastavi dalje
				await _next(context);
				return;
			}

			string? currentRegion = await _geoLocationService.GetRegionByIpAsync(ipAddress);
			if (string.IsNullOrEmpty(currentRegion))
			{
				// Nismo uspeli da dobijemo region, nastavi dalje
				await _next(context);
				return;
			}

			// Dohvati nedavne lokacije korisnika iz UserLocations u poslednjih 5 minuta
			var recentLocations = await _unitOfWork.UserLocations.GetRecentLocationsAsync(userId, LocationCheckInterval);

			// Dodaj novu lokaciju (IP, region, vreme pristupa)
			var newUserLocation = new UserLocation
			{
				UserId = userId,
				IpAddress = ipAddress,
				Region = currentRegion,
				AccessTime = DateTime.UtcNow
			};

			await _unitOfWork.UserLocations.AddAsync(newUserLocation);
			await _unitOfWork.SaveChangesAsync();

			// Izračunaj koliko različitih regiona ima u poslednjih 5 minuta + trenutni pristup
			var distinctRegions = recentLocations
				.Select(ul => ul.Region)
				.Append(currentRegion)
				.Distinct(StringComparer.OrdinalIgnoreCase)
				.ToList();

			if (distinctRegions.Count > 1)
			{
				var user = await _unitOfWork.Users.GetByIdAsync(userId);
				if (user != null && user.UserStatus == UserStatus.Active)
				{
					user.UserStatus = UserStatus.Inactive;
					_unitOfWork.Users.Update(user);
					await _unitOfWork.SaveChangesAsync();

					_logger.LogWarning("User {UserId} account locked due to multiple geo-locations in {Interval} minutes: {Regions}",
						userId, LocationCheckInterval.TotalMinutes, string.Join(", ", distinctRegions));

					context.Response.StatusCode = StatusCodes.Status423Locked;
					await context.Response.WriteAsync("Your account has been locked due to suspicious activity from multiple locations. Please verify your identity.");
					return;
				}
			}

			// Nastavi dalje u pipeline
			await _next(context);
		}
	}
}
