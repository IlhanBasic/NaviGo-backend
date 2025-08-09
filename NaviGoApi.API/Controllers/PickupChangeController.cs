using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.PickupChange;
using NaviGoApi.Application.CQRS.Queries.PickupChange;
using NaviGoApi.Application.DTOs.PickupChange;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PickupChangeController : ControllerBase
	{
		private readonly IMediator _mediator;

		public PickupChangeController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var pickupChanges = await _mediator.Send(new GetAllPickupChangesQuery());
			return Ok(pickupChanges);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var pickupChange = await _mediator.Send(new GetPickupChangeByIdQuery(id));
			if (pickupChange == null)
				return NotFound(new { message = $"Pickup change with id {id} not found." });

			return Ok(pickupChange);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] PickupChangeCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var newId = await _mediator.Send(new AddPickupChangeCommand(dto));
			return Ok(new { message = "Pickup change created successfully.", id = newId });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] PickupChangeUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _mediator.Send(new UpdatePickupChangeCommand(id, dto));
			return Ok(new { message = $"Pickup change with id {id} updated successfully." });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeletePickupChangeCommand(id));
			return Ok(new { message = $"Pickup change with id {id} deleted successfully." });
		}
	}
}
