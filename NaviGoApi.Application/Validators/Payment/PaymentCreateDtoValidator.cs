using FluentValidation;
using NaviGoApi.Application.DTOs.Payment;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Application.Validators.Payment
{
	public class PaymentCreateDtoValidator : AbstractValidator<PaymentCreateDto>
	{
		public PaymentCreateDtoValidator()
		{
			RuleFor(x => x.ContractId)
				.GreaterThan(0).WithMessage("ContractId must be greater than 0.");

			RuleFor(x => x.ClientId)
				.GreaterThan(0).WithMessage("ClientId must be greater than 0.");

			RuleFor(x => x.Amount)
				.GreaterThan(0).WithMessage("Amount must be greater than 0.");

			RuleFor(x => x.PaymentDate)
				.LessThanOrEqualTo(DateTime.UtcNow).WithMessage("PaymentDate cannot be in the future.");

			// ReceiptUrl je opcionalno, ali ako postoji, možeš dodati pravila za format URL-a itd.
			RuleFor(x => x.ReceiptUrl)
				.Must(uri => string.IsNullOrEmpty(uri) || Uri.IsWellFormedUriString(uri, UriKind.Absolute))
				.WithMessage("ReceiptUrl must be a valid URL.");
		}
	}
}
