using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Interfaces;

public class GeoLocationValidationMiddleware
{
	private readonly RequestDelegate _next;
	private readonly IGeoLocationService _geoLocationService;
	private readonly ILogger<GeoLocationValidationMiddleware> _logger;
	private readonly IUnitOfWork _unitOfWork;

	private static readonly HashSet<string> _blockedRegions = new()
	{
		"BlockedCountryCode1",
		"BlockedCountryCode2"
	};

	public GeoLocationValidationMiddleware(RequestDelegate next,
		IGeoLocationService geoLocationService,
		IUnitOfWork unitOfWork,
		ILogger<GeoLocationValidationMiddleware> logger)
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

		string? region = await _geoLocationService.GetRegionByIpAsync(ipAddress);

		if (region != null && _blockedRegions.Contains(region))
		{
			_logger.LogWarning("Access denied from blocked region {Region} for user {UserId}, IP: {Ip}", region, userIdClaim, ipAddress);

			if (int.TryParse(userIdClaim, out var userId))
			{
				// Log suspicious attempt (ovde samo primer, možeš da napraviš novu tabelu za logove)
				var user = await _unitOfWork.Users.GetByIdAsync(userId);
				if (user != null)
				{
					// Npr. možeš kreirati i sačuvati log o pokušaju (nije implementirano)
				}
			}

			context.Response.StatusCode = StatusCodes.Status403Forbidden;
			await context.Response.WriteAsync("Access denied: your region is not allowed.");
			return;
		}

		await _next(context);
	}
}
