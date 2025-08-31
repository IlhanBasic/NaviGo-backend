using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;
using System.ComponentModel.DataAnnotations;

public class SessionLockMiddleware
{
	private readonly RequestDelegate _next;
	private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
	private readonly IEmailService _emailService;

	public SessionLockMiddleware(RequestDelegate next, IEmailService emailService)
	{
		_next = next;
		_emailService = emailService;
	}

	public async Task InvokeAsync(HttpContext context)
	{
		var unitOfWork = context.RequestServices.GetRequiredService<IUnitOfWork>();

		if (context.User.Identity?.IsAuthenticated == true)
		{
			var email = context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value;
			if (!string.IsNullOrWhiteSpace(email))
			{
				var user = await unitOfWork.Users.GetByEmailAsync(email);
				if (user != null && user.UserStatus != UserStatus.Inactive)

				{
					var recentLocations = await unitOfWork.UserLocations
						.GetRecentLocationsAsync(user.Id, _interval);

					var distinctRegions = recentLocations
						.Select(l => l.Region)
						.Distinct()
						.ToList();

					if (distinctRegions.Count > 1)
					{
						user.UserStatus = UserStatus.Inactive;
						await unitOfWork.SaveChangesAsync();
						await _emailService.SendSuspendEmailAsync(user.Email);

						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						await context.Response.WriteAsync("Your account has been locked due to suspicious activity.");
						throw new ValidationException("Your account has been locked due to suspicious activity.");
						
					}
				}
			}
		}

		await _next(context);
	}
}
