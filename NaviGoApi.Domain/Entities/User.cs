using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NaviGoApi.Domain.Entities
{
	public enum UserRole
	{
		RegularUser = 1,
		CompanyUser = 2,
		CompanyAdmin = 3,
		SuperAdmin = 4
	}

	public enum UserStatus
	{
		Inactive = 0,
		Active = 1
	}

	public class User
	{
		public int Id { get; set; }

		public string Email { get; set; }

		public string PasswordHash { get; set; }

		public string FirstName { get; set; }

		public string LastName { get; set; }

		public string PhoneNumber { get; set; }

		public UserRole UserRole { get; set; }

		public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

		public DateTime? LastLogin { get; set; }

		public bool EmailVerified { get; set; }
		public string? EmailVerificationToken { get; set; }
		public TimeSpan? EmailVerificationTokenDuration { get; set; } = TimeSpan.FromMinutes(15);

		public string? PasswordResetToken {  get; set; }
		public TimeSpan? PasswordResetTokenDuration { get; set; } = TimeSpan.FromMinutes(15);
		public UserStatus UserStatus { get; set; } = UserStatus.Active;

		public int? CompanyId { get; set; }

		// Navigation properties
		public Company? Company { get; set; }

		public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

		public ICollection<VehicleMaintenance> VehicleMaintenancesReported { get; set; } = new List<VehicleMaintenance>();

		public ICollection<ShipmentDocument> ShipmentDocumentsVerified { get; set; } = new List<ShipmentDocument>();

		public ICollection<ShipmentStatusHistory> ShipmentStatusChanges { get; set; } = new List<ShipmentStatusHistory>();
	}

}
