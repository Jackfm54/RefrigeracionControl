
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Domain.Entities;
using RefrigeracionControl.Infrastructure.Identity;

namespace RefrigeracionControl.Infrastructure.Data;

public class ApplicationDbContext
	: IdentityDbContext<ApplicationUser>
{
	public ApplicationDbContext(
		DbContextOptions<ApplicationDbContext> options)
		: base(options)
	{
	}


	public DbSet<WorkOrder> WorkOrders
		=> Set<WorkOrder>();


	public DbSet<SupervisorTechnicianAssignment>
		SupervisorTechnicianAssignments => Set<
			SupervisorTechnicianAssignment>();

	protected override void OnModelCreating(
		ModelBuilder builder)
	{
		base.OnModelCreating(builder);

		builder.Entity<WorkOrder>(entity =>
		{
			entity.HasKey(x => x.Id);

			entity.Property(x => x.Title)
				.IsRequired()
				.HasMaxLength(180);

			entity.Property(x => x.Description)
				.IsRequired()
				.HasMaxLength(4000);

			entity.Property(x => x.ServiceType)
				.IsRequired()
				.HasMaxLength(60);

			entity.Property(x => x.Status)
				.HasConversion<string>()
				.HasMaxLength(30);

			entity.HasOne<ApplicationUser>()
				.WithMany()
				.HasForeignKey(x => x.SupervisorId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasOne<ApplicationUser>()
				.WithMany()
				.HasForeignKey(x => x.TechnicianId)
				.OnDelete(DeleteBehavior.Restrict);

			entity.HasIndex(x => x.TechnicianId);
			entity.HasIndex(x => x.SupervisorId);
			entity.HasIndex(x => x.Status);
		});

		builder.Entity<SupervisorTechnicianAssignment>(
			entity =>
			{
				entity.HasKey(x => x.Id);

				entity.Property(x => x.SupervisorId)
					.IsRequired();

				entity.Property(x => x.TechnicianId)
					.IsRequired();

				entity.HasIndex(x => x.TechnicianId)
					.IsUnique();

				entity.HasIndex(x => x.SupervisorId);

				entity.HasOne<ApplicationUser>()
					.WithMany()
					.HasForeignKey(x => x.SupervisorId)
					.OnDelete(DeleteBehavior.Restrict);

				entity.HasOne<ApplicationUser>()
					.WithMany()
					.HasForeignKey(x => x.TechnicianId)
					.OnDelete(DeleteBehavior.Restrict);
			});
	}
}
