using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.User;
using NaviGoApi.Application.CQRS.Queries.User;
using NaviGoApi.Application.DTOs.User;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IMediator _mediator;

		public UserController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// POST: api/user
		[HttpPost]
		public async Task<ActionResult<UserDto>> AddUser([FromBody] UserCreateDto userCreateDto)
		{
			var command = new AddUserCommand(userCreateDto);
			var createdUser = await _mediator.Send(command);
			return CreatedAtAction(nameof(GetUserById), new { id = createdUser.Id }, createdUser);
		}

		// GET: api/user/{id}
		[HttpGet("{id}")]
		public async Task<ActionResult<UserDto>> GetUserById(int id)
		{
			var query = new GetUserByIdQuery(id);
			var user = await _mediator.Send(query);
			if (user == null)
				return NotFound();
			return Ok(user);
		}

		// GET: api/user
		[HttpGet]
		public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
		{
			var query = new GetAllUserQuery();
			var users = await _mediator.Send(query);
			return Ok(users);
		}

		// DELETE: api/user/{id}
		[HttpDelete("{id}")]
		public async Task<ActionResult> DeleteUser(int id)
		{
			var command = new DeleteUserCommand(id);
			var result = await _mediator.Send(command);
			if (!result)
				return NotFound();
			return NoContent();
		}
		[HttpGet("verify-email")]
		public async Task<IActionResult> VerifyEmail([FromQuery] string token)
		{
			var command = new VerifyEmailCommand(token);
			var result = await _mediator.Send(command);
			if (!result)
				return BadRequest("Email Verification Failed: Invalid or expired token.");
			if (!result)
			{
				var errorHtml = @"
            <html>
            <head>
                <title>Email Verification Failed</title>
                <style>
                    body { font-family: Arial, sans-serif; background-color: #f8d7da; color: #721c24; text-align: center; padding: 50px; }
                    .container { background-color: #f5c6cb; border-radius: 10px; padding: 20px; max-width: 500px; margin: auto; }
                    a { color: #721c24; text-decoration: none; font-weight: bold; }
                    a:hover { text-decoration: underline; }
                </style>
            </head>
            <body>
                <div class='container'>
                    <h1>Email Verification Failed</h1>
                    <p>Invalid or expired verification token.</p>
                    <p><a href='/login'>Go to Login</a></p>
                </div>
            </body>
            </html>";
				return Content(errorHtml, "text/html");
			}

			var successHtml = @"
        <html>
        <head>
            <title>Email Verified</title>
            <style>
                body { font-family: Arial, sans-serif; background-color: #d4edda; color: #155724; text-align: center; padding: 50px; }
                .container { background-color: #c3e6cb; border-radius: 10px; padding: 20px; max-width: 500px; margin: auto; }
                a { color: #155724; text-decoration: none; font-weight: bold; }
                a:hover { text-decoration: underline; }
            </style>
        </head>
        <body>
            <div class='container'>
                <h1>Success!</h1>
                <p>Your email has been verified successfully.</p>
                <p><a href='/login'>Proceed to Login</a></p>
            </div>
        </body>
        </html>";
			return Content(successHtml, "text/html");
		}
		[HttpPost("forgot-password")]
		public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
		{
			var result = await _mediator.Send(new ForgotPasswordCommand(request.Email));
			if (!result)
				return BadRequest(new { message = "Unable to process forgot password request." });
			return Ok(new { message = "If an account with that email exists, a password reset link has been sent." });
		}

		[HttpPost("reset-password")]
		public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
		{
			var result = await _mediator.Send(new ResetPasswordCommand(request.Token, request.NewPassword));
			if (!result)
				return BadRequest(new { message = "Invalid token or password reset failed." });
			return Ok(new { message = "Password has been reset successfully." });
		}

		[HttpPost("change-password/{id}")]
		public async Task<IActionResult> ChangePassword(int id,[FromBody] ChangePasswordRequestDto request)
		{
			var result = await _mediator.Send(new ChangePasswordCommand(id, request.CurrentPassword, request.NewPassword));
			if (!result)
				return BadRequest(new { message = "Current password is incorrect or password change failed." });
			return Ok(new { message = "Password has been changed successfully." });
		}
	}
}
