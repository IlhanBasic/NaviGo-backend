using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.ForwarderOffer;
using NaviGoApi.Application.CQRS.Queries.ForwarderOffer;
using NaviGoApi.Application.DTOs.ForwarderOffer;
using System.Collections.Generic;
using System.Threading.Tasks;

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
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllForwarderOfferQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetForwarderOfferByIdQuery(id));
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] ForwarderOfferCreateDto dto)
		{
			await _mediator.Send(new AddForwarderOfferCommand(dto));
			return NoContent();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] ForwarderOfferUpdateDto dto)
		{
			await _mediator.Send(new UpdateForwarderOfferCommand(id, dto));
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteForwarderOfferCommand(id));
			return NoContent();
		}
	}
}
