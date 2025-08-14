using Microsoft.AspNetCore.Http;
using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;

public class GeoLocationValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IGeoLocationService _geoLocationService;

	// Zabranjene zemlje
	private readonly string[] _blockedCountries = { "US", "DE" };

	public GeoLocationValidationMiddleware(RequestDelegate next, IGeoLocationService geoLocationService)
	{
		_next = next;
		_geoLocationService = geoLocationService;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var unitOfWork = context.RequestServices.GetRequiredService<IUnitOfWork>();

		var ipAddress = context.Request.Headers["X-Forwarded-For"].FirstOrDefault()
				?? context.Connection.RemoteIpAddress?.ToString()
				?? "unknown";

		var region = await _geoLocationService.GetRegionByIpAsync(ipAddress);


		if (region != null && _blockedCountries.Contains(region))
		{
			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			await context.Response.WriteAsync("Access denied from your region.");
			return;
		}

		if (context.User.Identity?.IsAuthenticated == true)
		{
			var userEmail = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (!string.IsNullOrWhiteSpace(userEmail))
			{
				var user = await unitOfWork.Users.GetByEmailAsync(userEmail);
				if (user != null)
				{
					var userLocation = new UserLocation
					{
						UserId = user.Id,
						IpAddress = ipAddress,
						Region = region ?? "Unknown",
						AccessTime = DateTime.UtcNow
					};
					user.UserLocations.Add(userLocation);
					await unitOfWork.SaveChangesAsync();
				}
			}
		}

		await _next(context);
	}
}
