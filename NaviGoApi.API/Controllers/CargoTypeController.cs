using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.CargoType;
using NaviGoApi.Application.CQRS.Queries.CargoType;
using NaviGoApi.Application.DTOs.CargoType;

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
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllCargoTypeQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetCargoTypeByIdQuery(id));
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] CargoTypeCreateDto dto)
		{
			await _mediator.Send(new AddCargoTypeCommand(dto));
			return NoContent();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] CargoTypeUpdateDto dto)
		{
			await _mediator.Send(new UpdateCargoTypeCommand(id, dto));
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteCargoTypeCommand(id));
			return NoContent();
		}
	}
}
