using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.VehicleType;
using NaviGoApi.Application.CQRS.Queries.VehicleType;
using NaviGoApi.Application.DTOs.VehicleType;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class VehicleTypeController : ControllerBase
	{
		private readonly IMediator _mediator;

		public VehicleTypeController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllVehicleTypeQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetVehicleTypeByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Vehicle type with ID {id} was not found." });

			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] VehicleTypeCreateDto dto)
		{
			await _mediator.Send(new AddVehicleTypeCommand(dto));
			return StatusCode(201, new { message = "Vehicle type created successfully." });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] VehicleTypeUpdateDto dto)
		{
			// Pretpostavljam da command baca izuzetak ili handler interno proverava postojanje entiteta.
			await _mediator.Send(new UpdateVehicleTypeCommand(id, dto));
			return Ok(new { message = $"Vehicle type with ID {id} updated successfully." });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteVehicleTypeCommand(id));
			return Ok(new { message = $"Vehicle type with ID {id} deleted successfully." });
		}
	}
}
