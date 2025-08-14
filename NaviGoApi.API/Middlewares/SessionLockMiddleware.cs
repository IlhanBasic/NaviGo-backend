using NaviGoApi.Application.Services;
using NaviGoApi.Domain.Entities;
using NaviGoApi.Domain.Interfaces;

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
				if (user != null)
				{
					var recentLocations = user.UserLocations
						.Where(l => l.AccessTime >= DateTime.UtcNow - _interval)
						.Select(l => l.Region)
						.Distinct()
						.ToList();

					if (recentLocations.Count > 1)
					{
						user.UserStatus = UserStatus.Inactive;
						await unitOfWork.SaveChangesAsync();
						await _emailService.SendVerificationEmailAsync(user.Email, "Your account has been locked due to suspicious activity.");
						context.Response.StatusCode = StatusCodes.Status403Forbidden;
						await context.Response.WriteAsync("Your account has been locked due to suspicious activity.");
						return;
					}
				}
			}
		}

		await _next(context);
	}
}
