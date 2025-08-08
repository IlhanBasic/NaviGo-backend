using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Payment;
using NaviGoApi.Application.CQRS.Queries.Payment;
using NaviGoApi.Application.DTOs.Payment;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class PaymentController : ControllerBase
	{
		private readonly IMediator _mediator;

		public PaymentController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<IActionResult> GetAll()
		{
			var result = await _mediator.Send(new GetAllPaymentQuery());
			return Ok(result);
		}

		[HttpGet("{id}")]
		public async Task<IActionResult> GetById(int id)
		{
			var result = await _mediator.Send(new GetPaymentByIdQuery(id));
			if (result == null) return NotFound();
			return Ok(result);
		}

		[HttpPost]
		public async Task<IActionResult> Create([FromBody] PaymentCreateDto dto)
		{
			await _mediator.Send(new AddPaymentCommand(dto));
			return NoContent();
		}

		[HttpPut("{id}")]
		public async Task<IActionResult> Update(int id, [FromBody] PaymentUpdateDto dto)
		{
			await _mediator.Send(new UpdatePaymentCommand(dto) { Id = id });
			return NoContent();
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			await _mediator.Send(new DeletePaymentCommand(id));
			return NoContent();
		}
	}
}
