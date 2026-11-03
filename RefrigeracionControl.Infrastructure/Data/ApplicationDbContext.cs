
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

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<RefrigerationEquipment> RefrigerationEquipments
        => Set<RefrigerationEquipment>();

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
            entity.Property(x => x.ReviewFeedback)
				  .HasMaxLength(2000);
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

        // CLIENTES
        builder.Entity<Client>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);

            entity.Property(x => x.ContactName)
                .HasMaxLength(150);

            entity.Property(x => x.Email)
                .HasMaxLength(254);

            entity.Property(x => x.Phone)
                .HasMaxLength(40);

            entity.Property(x => x.Address)
                .HasMaxLength(300);

            entity.Property(x => x.City)
                .HasMaxLength(120);

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.HasIndex(x => x.Name);
        });

        // EQUIPOS
        builder.Entity<RefrigerationEquipment>(entity =>
        {
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(180);

            entity.Property(x => x.EquipmentType)
                .IsRequired()
                .HasMaxLength(100);

            entity.Property(x => x.Brand)
                .HasMaxLength(100);

            entity.Property(x => x.Model)
                .HasMaxLength(100);

            entity.Property(x => x.SerialNumber)
                .HasMaxLength(150);

            entity.Property(x => x.Location)
                .HasMaxLength(200);

            entity.Property(x => x.RefrigerantType)
                .HasMaxLength(80);

            entity.Property(x => x.Notes)
                .HasMaxLength(2000);

            entity.HasOne(x => x.Client)
                .WithMany(x => x.Equipments)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(x => x.SerialNumber);
        });

        // RELACIONES OPCIONALES DE ÓRDENES
        builder.Entity<WorkOrder>(entity =>
        {
            entity.HasOne<Client>()
                .WithMany()
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne<RefrigerationEquipment>()
                .WithMany()
                .HasForeignKey(x => x.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
