using FluentValidation;
using NaviGoApi.Application.DTOs.User;
using NaviGoApi.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.User
{
	public class UserUpdateDtoValidator: AbstractValidator<UserCreateDto>
	{
        public UserUpdateDtoValidator()
        {
			RuleFor(x => x.FirstName)
				.NotEmpty().WithMessage("First name is required.");

			RuleFor(x => x.LastName)
				.NotEmpty().WithMessage("Last name is required.");

			RuleFor(x => x.PhoneNumber)
				.NotEmpty().WithMessage("Phone number is required.");
		}
    }
}
