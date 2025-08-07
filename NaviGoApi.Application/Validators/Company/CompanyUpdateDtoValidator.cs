using FluentValidation;
using NaviGoApi.Application.DTOs.Company;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Company
{
	public class CompanyUpdateDtoValidator : AbstractValidator<CompanyUpdateDto>
	{
		public CompanyUpdateDtoValidator()
		{
			RuleFor(x => x.Id)
				.GreaterThan(0).WithMessage("Id must be greater than zero.");

			RuleFor(x => x.CompanyName)
				.NotEmpty().WithMessage("Company name is required.")
				.MaximumLength(200).WithMessage("Company name cannot exceed 200 characters.");

			RuleFor(x => x.PIB)
				.NotEmpty().WithMessage("PIB is required.")
				.Matches(@"^\d{9,13}$").WithMessage("PIB must be between 9 and 13 digits.");

			RuleFor(x => x.Address)
				.NotEmpty().WithMessage("Address is required.")
				.MaximumLength(300).WithMessage("Address cannot exceed 300 characters.");

			RuleFor(x => x.ContactEmail)
				.NotEmpty().WithMessage("Contact email is required.")
				.EmailAddress().WithMessage("Invalid email format.");

			RuleFor(x => x.Website)
				.MaximumLength(200).WithMessage("Website URL cannot exceed 200 characters.")
				.Must(uri => Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				.When(x => !string.IsNullOrEmpty(x.Website))
				.WithMessage("Invalid website URL.");

			RuleFor(x => x.Description)
				.MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");

			RuleFor(x => x.CompanyType)
				.IsInEnum().WithMessage("Invalid company type.");

			RuleFor(x => x.CompanyStatus)
				.IsInEnum().WithMessage("Invalid company status.");

			RuleFor(x => x.MaxCommissionRate)
				.GreaterThanOrEqualTo(0).WithMessage("Max commission rate must be non-negative.")
				.LessThanOrEqualTo(100).WithMessage("Max commission rate cannot exceed 100%")
				.When(x => x.MaxCommissionRate.HasValue);

			RuleFor(x => x.SaldoAmount)
				.GreaterThanOrEqualTo(0).WithMessage("Saldo amount must be non-negative.")
				.When(x => x.SaldoAmount.HasValue);

			RuleFor(x => x.ProofFileUrl)
				.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
				.When(x => !string.IsNullOrEmpty(x.ProofFileUrl))
				.WithMessage("Proof file URL must be a valid URL.");
		}
	}
}
