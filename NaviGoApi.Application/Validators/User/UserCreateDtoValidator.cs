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
			RuleFor(x => x)
				.Must(dto =>
				{
					return dto.UserRole switch
					{
						UserRole.SuperAdmin => dto.CompanyId == null,
						UserRole.CompanyUser or UserRole.CompanyAdmin => dto.CompanyId != null,
						UserRole.RegularUser => true,
						_ => true
					};
				})
				.WithMessage("Invalid CompanyId based on UserRole.");


			RuleFor(x => x.UserRole)
				.IsInEnum();
		}
	}
}
