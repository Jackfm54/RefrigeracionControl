
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Tecnico")]
public class MySupervisorController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public MySupervisorController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var technicianId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(technicianId))
            return Challenge();

        var supervisor = await (
            from assignment in _db.SupervisorTechnicianAssignments
            join user in _db.Users
                on assignment.SupervisorId equals user.Id
            where assignment.TechnicianId == technicianId
                  && user.IsActive
            select new SupervisorInfoViewModel
            {
                FullName = user.FullName,
                Email = user.Email ?? "",
                IsAssigned = true
            }
        ).AsNoTracking().SingleOrDefaultAsync();

        return View(supervisor ?? new SupervisorInfoViewModel());
    }
}
