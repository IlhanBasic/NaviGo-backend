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
			RuleFor(x => x.Amount)
				.GreaterThan(0).WithMessage("Amount must be greater than 0.");

			RuleFor(x => x.PaymentDate)
				.LessThanOrEqualTo(DateTime.UtcNow).WithMessage("PaymentDate cannot be in the future.");

			RuleFor(x => x.PaymentStatus)
				.IsInEnum().WithMessage("PaymentStatus must be a valid enum value.");

			RuleFor(x => x.ReceiptUrl)
				.Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				.WithMessage("ReceiptUrl must be a valid URL.");
		}
	}
}
