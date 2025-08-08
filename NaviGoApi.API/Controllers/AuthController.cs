using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.DTOs.User;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IMediator _mediator;

		public AuthController(IMediator mediator)
		{
			_mediator = mediator;
		}

		/// <summary>
		/// Login user and receive JWT tokens.
		/// </summary>
		[HttpPost("login")]
		public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
		{
			var result = await _mediator.Send(new AuthenticateCommand(request.Email, request.Password));
			if (result == null)
				return Unauthorized(new
				{
					error = "Unauthorized",
					message = "Invalid email or password. Please check your credentials and try again."
				});

			return Ok(new
			{
				accessToken = result.Value.accessToken,
				refreshToken = result.Value.refreshToken
			});
		}

		/// <summary>
		/// Refresh JWT token using a valid refresh token.
		/// </summary>
		[HttpPost("refresh")]
		public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
		{
			var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

			var result = await _mediator.Send(new RefreshTokenCommand(request.Token, ipAddress));

			if (result == null)
				return Unauthorized(new
				{
					error = "Unauthorized",
					message = "The refresh token is invalid or has expired. Please login again."
				});

			return Ok(new
			{
				accessToken = result.Value.accessToken,
				refreshToken = result.Value.refreshToken
			});
		}

		/// <summary>
		/// Login with Google token and receive JWT tokens.
		/// </summary>
		[HttpPost("google-login")]
		public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
		{
			var result = await _mediator.Send(new GoogleAuthenticateCommand(request.IdToken));
			if (result == null)
				return Unauthorized(new
				{
					error = "Unauthorized",
					message = "Invalid Google token provided. Please try again."
				});

			return Ok(new
			{
				accessToken = result.Value.accessToken,
				refreshToken = result.Value.refreshToken
			});
		}
	}
}
