using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Shipment;
using NaviGoApi.Application.CQRS.Queries.Shipment;
using NaviGoApi.Application.DTOs.Shipment;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ShipmentController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ShipmentController(IMediator mediator)
		{
			_mediator = mediator;
		}

		// GET: api/Shipment
		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllShipmentQuery());
			return Ok(result);
		}

		// GET: api/Shipment/{id}
		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetShipmentByIdQuery(id));
			if (result == null)
			{
				return NotFound(new
				{
					Message = $"Shipment with ID {id} was not found."
				});
			}

			return Ok(result);
		}

		// POST: api/Shipment
		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] ShipmentCreateDto dto)
		{
			await _mediator.Send(new AddShipmentCommand(dto));
			return Ok(new
			{
				Message = "Shipment created successfully."
			});
		}

		// PUT: api/Shipment/{id}
		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] ShipmentUpdateDto dto)
		{
			await _mediator.Send(new UpdateShipmentCommand(id, dto));
			return Ok(new
			{
				Message = $"Shipment with ID {id} updated successfully."
			});
		}

		// DELETE: api/Shipment/{id}
		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteShipmentCommand(id));
			return Ok(new
			{
				Message = $"Shipment with ID {id} deleted successfully."
			});
		}
	}
}
