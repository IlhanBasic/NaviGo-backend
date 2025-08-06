using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NaviGoApi.Domain.Entities;

namespace NaviGoApi.Infrastructure.Postgresql.Persistence.Configurations
{
	internal class VehicleMaintenanceConfiguration : IEntityTypeConfiguration<VehicleMaintenance>
	{
		public void Configure(EntityTypeBuilder<VehicleMaintenance> builder)
		{

			builder.HasKey(vm => vm.Id);

			builder.Property(vm => vm.Description)
				.IsRequired()
				.HasMaxLength(1000);

			builder.Property(vm => vm.ReportedAt)
				.IsRequired();

			builder.Property(vm => vm.ResolvedAt)
				.IsRequired(false);

			builder.Property(vm => vm.Severity)
				.IsRequired();

			builder.Property(vm => vm.RepairCost)
				.IsRequired(false)
				.HasColumnType("decimal(18,2)");

			builder.Property(vm => vm.MaintenanceType)
				.IsRequired();

			// Relations

			// VehicleMaintenance (M) : Vehicle (1)
			builder.HasOne(vm => vm.Vehicle)
				.WithMany(v => v.VehicleMaintenances)
				.HasForeignKey(vm => vm.VehicleId)
				.OnDelete(DeleteBehavior.Cascade);

			// VehicleMaintenance (M) : User (1) ReportedByUser
			builder.HasOne(vm => vm.ReportedByUser)
				.WithMany(u => u.VehicleMaintenancesReported)
				.HasForeignKey(vm => vm.ReportedByUserId)
				.OnDelete(DeleteBehavior.Restrict);
		}
	}
}
