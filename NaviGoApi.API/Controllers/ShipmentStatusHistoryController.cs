using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.ShipmentStatusHistory;
using NaviGoApi.Application.CQRS.Queries.ShipmentStatusHistory;
using NaviGoApi.Application.DTOs.ShipmentStatusHistory;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShipmentStatusHistoryController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ShipmentStatusHistoryController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/ShipmentStatusHistory
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var histories = await _mediator.Send(new GetAllShipmentStatusHistoryQuery());
			return Ok(histories);
		}

		// GET: api/ShipmentStatusHistory/5
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var history = await _mediator.Send(new GetShipmentStatusHistoryByIdQuery(id));
			if (history == null)
				return NotFound(new { message = $"Shipment status history with id {id} not found." });

			return Ok(history);
		}

		// POST: api/ShipmentStatusHistory
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] ShipmentStatusHistoryCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _mediator.Send(new AddShipmentStatusHistoryCommand(dto));
			return Ok(new
			{
				message = "Shipment status history created successfully."
			});
		}

		// PUT: api/ShipmentStatusHistory/5
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] ShipmentStatusHistoryUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _mediator.Send(new UpdateShipmentStatusHistoryCommand(id, dto));
			return Ok(new { message = "Shipment status history updated successfully." });
		}

		// DELETE: api/ShipmentStatusHistory/5
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteShipmentStatusHistoryCommand(id));
			return Ok(new { message = "Shipment status history deleted successfully." });
		}
	}
}
