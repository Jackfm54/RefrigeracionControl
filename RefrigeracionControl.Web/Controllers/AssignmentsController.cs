
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Domain.Entities;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class AssignmentsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AssignmentsController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SupervisorAssignmentViewModel();

        await LoadOptions(model);

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        SupervisorAssignmentViewModel model)
    {
        if (model.SupervisorId == model.TechnicianId)
        {
            ModelState.AddModelError(
                "",
                "Un usuario no puede supervisarse a sí mismo.");
        }

        var supervisor = string.IsNullOrWhiteSpace(model.SupervisorId)
            ? null
            : await _users.FindByIdAsync(model.SupervisorId);

        var technician = string.IsNullOrWhiteSpace(model.TechnicianId)
            ? null
            : await _users.FindByIdAsync(model.TechnicianId);

        if (supervisor == null ||
            !supervisor.IsActive ||
            !await _users.IsInRoleAsync(
                supervisor, "Supervisor"))
        {
            ModelState.AddModelError(
                nameof(model.SupervisorId),
                "Seleccione un supervisor activo válido.");
        }

        if (technician == null ||
            !technician.IsActive ||
            !await _users.IsInRoleAsync(
                technician, "Tecnico"))
        {
            ModelState.AddModelError(
                nameof(model.TechnicianId),
                "Seleccione un técnico activo válido.");
        }

        if (!ModelState.IsValid)
        {
            await LoadOptions(model);
            return View(model);
        }

        var assignment = await _db
            .SupervisorTechnicianAssignments
            .SingleOrDefaultAsync(
                a => a.TechnicianId == model.TechnicianId);

        if (assignment == null)
        {
            _db.SupervisorTechnicianAssignments.Add(
                new SupervisorTechnicianAssignment
                {
                    SupervisorId = model.SupervisorId,
                    TechnicianId = model.TechnicianId,
                    AssignedAtUtc = DateTime.UtcNow
                });
        }
        else
        {
            assignment.SupervisorId = model.SupervisorId;
            assignment.AssignedAtUtc = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Técnico asignado correctamente.";

        return RedirectToAction(
            "Index", "Technicians");
    }

    private async Task LoadOptions(
        SupervisorAssignmentViewModel model)
    {
        var supervisors = await _users
            .GetUsersInRoleAsync("Supervisor");

        var technicians = await _users
            .GetUsersInRoleAsync("Tecnico");

        model.Supervisors = supervisors
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new AssignmentOption
            {
                Id = u.Id,
                Name = u.FullName
            })
            .ToList();

        model.Technicians = technicians
            .Where(u => u.IsActive)
            .OrderBy(u => u.FullName)
            .Select(u => new AssignmentOption
            {
                Id = u.Id,
                Name = u.FullName
            })
            .ToList();
    }
}
