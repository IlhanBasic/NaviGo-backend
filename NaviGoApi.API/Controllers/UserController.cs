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
	}
}
