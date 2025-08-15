using FluentValidation;
using NaviGoApi.Application.DTOs.User;

namespace NaviGoApi.Application.Validators.User
{
	public class UserSuperAdminCreateDtoValidator : AbstractValidator<UserSuperAdminCreateDto>
	{
		public UserSuperAdminCreateDtoValidator()
		{
			RuleFor(x => x.Email)
				.NotEmpty().WithMessage("Email is required")
				.EmailAddress().WithMessage("Invalid email format");

			RuleFor(x => x.Password)
				.NotEmpty().WithMessage("Password is required")
				.MinimumLength(8).WithMessage("Password must be at least 8 characters");

			RuleFor(x => x.FirstName).NotEmpty();
			RuleFor(x => x.LastName).NotEmpty();
			RuleFor(x => x.PhoneNumber).NotEmpty();
		}
	}
}
