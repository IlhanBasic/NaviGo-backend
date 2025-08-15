using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Application.CQRS.Queries.CargoType;
using NaviGoApi.Application.DTOs.CargoType;
using System.Collections.Generic; // Za KeyNotFoundException

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CargoTypeController : ControllerBase
	{
		private readonly IMediator _mediator;

		public CargoTypeController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllCargoTypeQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetCargoTypeByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Cargo type with ID {id} not found." });
			return Ok(result);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] CargoTypeCreateDto dto)
		{
			await _mediator.Send(new AddCargoTypeCommand(dto));
			return NoContent();
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] CargoTypeUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateCargoTypeCommand(id, dto));
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Cargo type with ID {id} not found." });
			}
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteCargoTypeCommand(id));
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Cargo type with ID {id} not found." });
			}
		}
	}
}
