using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Application.CQRS.Queries.ForwarderOffer;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using Microsoft.AspNetCore.Authorization;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ForwarderOfferController : ControllerBase
	{
		private readonly IMediator _mediator;

		public ForwarderOfferController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		[Authorize]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllForwarderOfferQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		[Authorize]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetForwarderOfferByIdQuery(id));
			if (result == null)
				return NotFound(new { error = "NotFound", message = $"Forwarder offer with ID {id} not found." });

			return Ok(result);
		}

		[HttpPost]
		[Authorize]
		public async Task<IActionResult> Create([FromBody] ForwarderOfferCreateDto dto)
		{
			await _mediator.Send(new AddForwarderOfferCommand(dto));
			return Ok(new { message = "Forwarder offer created successfully." });
		}

		[HttpPut("{id}")]
		[Authorize]
		public async Task<IActionResult> Update(int id, [FromBody] ForwarderOfferUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateForwarderOfferCommand(id, dto));
				return Ok(new { message = "Forwarder offer updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Forwarder offer with ID {id} not found." });
			}
		}

		[HttpDelete("{id}")]
		[Authorize]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteForwarderOfferCommand(id));
				return Ok(new { message = "Forwarder offer deleted successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Forwarder offer with ID {id} not found." });
			}
		}
		[HttpPut("{id}/status")]
		[Authorize]
		public async Task<IActionResult> UpdateStatus(int id, [FromBody] ForwarderOfferStatusUpdateDto dto)
		{
			try
			{
				await _mediator.Send(new UpdateForwarderOfferStatusCommand(id,dto));
				return Ok(new { message = "Forwarder offer status updated successfully." });
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Forwarder offer with ID {id} not found." });
			}
		}
	}
}
