using FluentValidation;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Application.DTOs.Company;
using NaviGoApi.Application.Validators.Company;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Application.Validators.User
{
	public class UserCreateDtoValidator : AbstractValidator<UserCreateDto>
	{
		public UserCreateDtoValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required.")
				.EmailAddress().WithMessage("Email must be valid.");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required.")
				.MinimumLength(6).WithMessage("Password must be at least 6 characters long.");

			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First name is required.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last name is required.");

			RuleFor(x => x.PhoneNumber)
				.NotEmpty().WithMessage("Phone number is required.");

			RuleFor(x => x.UserRole)
				.IsInEnum();

			RuleFor(x => x.UserStatus)
				.IsInEnum();
			RuleFor(x => x.CompanyId)
				.Must((dto, companyId) =>
				{
					if (dto.UserRole == UserRole.SuperAdmin)
						return companyId == null;
					return true;
				})
				.WithMessage("SuperAdmin users must not be associated with a company.");
		}
	}
}
