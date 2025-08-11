using FluentValidation;
using NaviGoApi.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Payment
{
	public class PaymentUpdateDtoValidator : AbstractValidator<PaymentUpdateDto>
	{
		public PaymentUpdateDtoValidator()
		{
			RuleFor(x => x.PaymentStatus)
				.IsInEnum().WithMessage("PaymentStatus must be a valid enum value.");
		}
	}
}
