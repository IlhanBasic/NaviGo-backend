using FluentValidation;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Company
{
	public class CompanyCreateDtoValidator : AbstractValidator<CompanyCreateDto>
	{
		public CompanyCreateDtoValidator()
		{
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

			RuleFor(x => x.MaxCommissionRate)
				.GreaterThanOrEqualTo(0).WithMessage("Max commission rate must be non-negative.")
				.LessThanOrEqualTo(100).WithMessage("Max commission rate cannot exceed 100%")
				.When(x => x.MaxCommissionRate.HasValue);
			RuleFor(x => x)
				.Must(x => x.CompanyType == CompanyType.Forwarder || x.MaxCommissionRate == null)
				.WithMessage("MaxCommissionRate can only be set if the company is a Forwarder.");
			RuleFor(x => x.ProofFileUrl)
				.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
				.When(x => !string.IsNullOrEmpty(x.ProofFileUrl))
				.WithMessage("Proof file URL must be a valid URL.");
		}
	}
}
