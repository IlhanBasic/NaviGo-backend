using MediatR;
using Microsoft.AspNetCore.Mvc;
using NaviGoApi.Application.CQRS.Commands.Company;
using NaviGoApi.Application.CQRS.Queries.Company;
using NaviGoApi.Application.DTOs.Company;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NaviGoApi.API.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class CompanyController : ControllerBase
	{
		private readonly IMediator _mediator;

		public CompanyController(IMediator mediator)
		{
			_mediator = mediator;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<CompanyDto>>> GetAll()
		{
			var companies = await _mediator.Send(new GetAllCompaniesQuery());
			return Ok(companies);
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<CompanyDto>> GetById(int id)
		{
			var company = await _mediator.Send(new GetCompanyByIdQuery(id));
			if (company == null)
				return NotFound(new { error = "NotFound", message = $"Company with ID {id} not found." });
			return Ok(company);
		}

		[HttpPost]
		public async Task<ActionResult<CompanyDto>> Create([FromBody] CompanyCreateDto dto)
		{
			var created = await _mediator.Send(new AddCompanyCommand(dto));
			// Pretpostavka: created nije null i sadrži novokreirani entitet s Id-em
			return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { message = "Company created successfully.", company = created });
		}

		[HttpPut("{id}")]
		public async Task<ActionResult<CompanyDto>> Update(int id, [FromBody] CompanyUpdateDto dto)
		{
			var updated = await _mediator.Send(new UpdateCompanyCommand(id, dto));
			if (updated == null)
				return NotFound(new { error = "NotFound", message = $"Company with ID {id} not found." });

			return Ok(new { message = "Company updated successfully.", company = updated });
		}

		[HttpDelete("{id}")]
		public async Task<IActionResult> Delete(int id)
		{
			try
			{
				await _mediator.Send(new DeleteCompanyCommand(id));
				return NoContent();
			}
			catch (KeyNotFoundException)
			{
				return NotFound(new { error = "NotFound", message = $"Company with ID {id} not found." });
			}
		}
	}
}
