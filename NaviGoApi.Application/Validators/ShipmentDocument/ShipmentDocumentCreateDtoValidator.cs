using FluentValidation;
using NaviGoApi.Application.DTOs.ShipmentDocument;

namespace NaviGoApi.Application.Validators.ShipmentDocument
{
	public class ShipmentDocumentCreateDtoValidator : AbstractValidator<ShipmentDocumentCreateDto>
	{
		public ShipmentDocumentCreateDtoValidator()
		{
			RuleFor(x => x.ShipmentId)
				.GreaterThan(0).WithMessage("ShipmentId must be greater than 0.");

			RuleFor(x => x.DocumentType)
				.IsInEnum().WithMessage("Invalid DocumentType.");

			RuleFor(x => x.FileUrl)
				.NotEmpty().WithMessage("FileUrl is required.")
				.Must(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
				.WithMessage("FileUrl must be a valid URL.");

			RuleFor(x => x.UploadDate)
				.LessThanOrEqualTo(DateTime.UtcNow)
				.WithMessage("UploadDate cannot be in the future.");

			// Verified i VerifiedByUserId mogu biti opciono, možeš dodati pravila po potrebi

			RuleFor(x => x.ExpiryDate)
				.GreaterThan(DateTime.UtcNow).When(x => x.ExpiryDate.HasValue)
				.WithMessage("ExpiryDate must be in the future.");
		}
	}
}
