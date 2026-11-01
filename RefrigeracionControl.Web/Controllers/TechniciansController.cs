
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor")]
public class TechniciansController : Controller
{
	private readonly UserManager<ApplicationUser> _users;
	private readonly ApplicationDbContext _db;

	public TechniciansController(
		UserManager<ApplicationUser> users,
		ApplicationDbContext db)
	{
		_users = users;
		_db = db;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var technicians = await _users
			.GetUsersInRoleAsync("Tecnico");

		var activeIds = technicians
			.Where(t => t.IsActive)
			.Select(t => t.Id)
			.ToList();

		if (!User.IsInRole("Administrador"))
		{
			var supervisorId = _users.GetUserId(User);

			if (string.IsNullOrEmpty(supervisorId))
				return Challenge();

			var assignedIds = await _db
				.SupervisorTechnicianAssignments
				.AsNoTracking()
				.Where(x => x.SupervisorId == supervisorId)
				.Select(x => x.TechnicianId)
				.ToListAsync();

			activeIds = activeIds
				.Intersect(assignedIds)
				.ToList();
		}

		var result = await _users.Users
			.AsNoTracking()
			.Where(u => activeIds.Contains(u.Id))
			.OrderBy(u => u.FullName)
			.Select(u => new TechnicianListViewModel
			{
				Id = u.Id,
				FullName = u.FullName,
				Email = u.Email ?? "",
				IsActive = u.IsActive
			})
			.ToListAsync();

		return View(result);
	}
}
