
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using RefrigeracionControl.Domain.Enums;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor,Tecnico")]
public class DashboardController : Controller
{
	private readonly ApplicationDbContext _db;
	private readonly UserManager<ApplicationUser> _users;

	public DashboardController(
		ApplicationDbContext db,
		UserManager<ApplicationUser> users)
	{
		_db = db;
		_users = users;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var userId = _users.GetUserId(User);

		if (string.IsNullOrWhiteSpace(userId))
			return Challenge();

		var isAdmin = User.IsInRole("Administrador");
		var isSupervisor = User.IsInRole("Supervisor");

		var orders = _db.WorkOrders.AsNoTracking();

		if (isSupervisor && !isAdmin)
		{
			orders = orders.Where(x =>
				x.SupervisorId == userId);
		}
		else if (!isAdmin)
		{
			orders = orders.Where(x =>
				x.TechnicianId == userId);
		}

		var model = new DashboardViewModel
		{
			DashboardScope = isAdmin
				? "Vista general de la empresa"
				: isSupervisor
					? "Órdenes de mi equipo"
					: "Mis órdenes de trabajo",

			TotalOrders = await orders.CountAsync(),

			PendingOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.Pending),

			AssignedOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.Assigned),

			InProgressOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.InProgress),

			UnderReviewOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.UnderReview),

			CompletedOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.Completed),

			CancelledOrders = await orders.CountAsync(
				x => x.Status == WorkOrderStatus.Cancelled)
		};

		if (isAdmin)
		{
			var technicians = await _users
				.GetUsersInRoleAsync("Tecnico");

			var supervisors = await _users
				.GetUsersInRoleAsync("Supervisor");

			model.ActiveTechnicians =
				technicians.Count(x => x.IsActive);

			model.ActiveSupervisors =
				supervisors.Count(x => x.IsActive);

			model.TotalUsers =
				await _users.Users.CountAsync();
		}
		else if (isSupervisor)
		{
			var technicians = await _users
				.GetUsersInRoleAsync("Tecnico");

			var assignedIds = await _db
				.SupervisorTechnicianAssignments
				.AsNoTracking()
				.Where(x => x.SupervisorId == userId)
				.Select(x => x.TechnicianId)
				.ToListAsync();

			model.ActiveTechnicians = technicians.Count(x =>
				x.IsActive && assignedIds.Contains(x.Id));
		}

		return View(model);
	}
}
