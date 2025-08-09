using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.DelayPenalty;
using NaviGoApi.Application.CQRS.Queries.DelayPenalty;
using NaviGoApi.Application.DTOs.DelayPenalty;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class DelayPenaltyController : ControllerBase
	{
		private readonly IMediator _mediator;

		public DelayPenaltyController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var penalties = await _mediator.Send(new GetAllDelayPenaltiesQuery());
			return Ok(penalties);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var penalty = await _mediator.Send(new GetDelayPenaltyByIdQuery(id));
			if (penalty == null)
				return NotFound(new { message = $"Delay penalty with ID {id} not found." });

			return Ok(penalty);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] DelayPenaltyCreateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _mediator.Send(new AddDelayPenaltyCommand(dto));
			return Ok(new { message = "Delay penalty created successfully." });
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] DelayPenaltyUpdateDto dto)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			await _mediator.Send(new UpdateDelayPenaltyCommand(id, dto));
			return Ok(new { message = "Delay penalty updated successfully." });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeleteDelayPenaltyCommand(id));
			return Ok(new { message = "Delay penalty deleted successfully." });
		}
	}
}
