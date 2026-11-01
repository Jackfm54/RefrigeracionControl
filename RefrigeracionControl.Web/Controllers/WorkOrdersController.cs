
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using RefrigeracionControl.Domain.Entities;
using RefrigeracionControl.Domain.Enums;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor,Tecnico")]
public class WorkOrdersController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public WorkOrdersController(
        ApplicationDbContext db,
        UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    // ==================================================
    // LISTADO DE ÓRDENES
    // ==================================================

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var query = _db.WorkOrders.AsNoTracking();

        if (User.IsInRole("Administrador"))
        {
            // El administrador consulta todas las órdenes.
        }
        else if (User.IsInRole("Supervisor"))
        {
            query = query.Where(
                x => x.SupervisorId == userId);
        }
        else if (User.IsInRole("Tecnico"))
        {
            query = query.Where(
                x => x.TechnicianId == userId);
        }
        else
        {
            return Forbid();
        }

        var orders = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync();

        var users = await _users.Users
            .AsNoTracking()
            .ToDictionaryAsync(
                x => x.Id,
                x => x.FullName);

        var result = orders.Select(order =>
            new WorkOrderListViewModel
            {
                Id = order.Id,
                Title = order.Title,
                ServiceType = order.ServiceType,
                Status = order.Status.ToString(),

                TechnicianName =
                    order.TechnicianId != null &&
                    users.TryGetValue(
                        order.TechnicianId,
                        out var technicianName)
                        ? technicianName
                        : "Sin asignar",

                CreatedAtUtc = order.CreatedAtUtc

            }).ToList();

        return View(result);
    }

    // ==================================================
    // FORMULARIO DE CREACIÓN
    // ==================================================

    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Create()
    {
        var model = new WorkOrderCreateViewModel();

        await LoadTechnicians(model);

        return View(model);
    }

    // ==================================================
    // GUARDAR NUEVA ORDEN
    // ==================================================

    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        WorkOrderCreateViewModel model)
    {
        var allowedTypes = new[]
        {
            "Preventivo",
            "Correctivo"
        };

        if (!allowedTypes.Contains(model.ServiceType))
        {
            ModelState.AddModelError(
                nameof(model.ServiceType),
                "Tipo de servicio no permitido.");
        }

        var currentUserId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(currentUserId))
            return Challenge();

        string? supervisorId =
            User.IsInRole("Supervisor")
                ? currentUserId
                : null;

        if (!string.IsNullOrWhiteSpace(model.TechnicianId))
        {
            var technician = await _users.FindByIdAsync(
                model.TechnicianId);

            if (technician == null ||
                !technician.IsActive ||
                !await _users.IsInRoleAsync(
                    technician, "Tecnico"))
            {
                ModelState.AddModelError(
                    nameof(model.TechnicianId),
                    "El técnico seleccionado no es válido.");
            }
            else
            {
                var assignment = await _db
                    .SupervisorTechnicianAssignments
                    .AsNoTracking()
                    .SingleOrDefaultAsync(
                        x => x.TechnicianId ==
                             model.TechnicianId);

                if (User.IsInRole("Supervisor"))
                {
                    if (assignment?.SupervisorId !=
                        currentUserId)
                    {
                        ModelState.AddModelError(
                            nameof(model.TechnicianId),
                            "El técnico no pertenece a su equipo.");
                    }
                }
                else
                {
                    supervisorId = assignment?.SupervisorId;
                }
            }
        }

        if (!ModelState.IsValid)
        {
            await LoadTechnicians(model);

            return View(model);
        }

        var order = new WorkOrder
        {
            Title = model.Title.Trim(),

            Description = model.Description.Trim(),

            ServiceType = model.ServiceType,

            TechnicianId = model.TechnicianId,

            SupervisorId = supervisorId,

            Status = string.IsNullOrEmpty(
                model.TechnicianId)
                ? WorkOrderStatus.Pending
                : WorkOrderStatus.Assigned,

            CreatedAtUtc = DateTime.UtcNow,

            ScheduledAtUtc =
                model.ScheduledAt?.ToUniversalTime()
        };

        _db.WorkOrders.Add(order);

        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Orden de servicio creada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // ==================================================
    // DETALLE DE LA ORDEN
    // ==================================================

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.WorkOrders
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == id);

        if (order == null)
            return NotFound();

        var userId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var isAdmin =
            User.IsInRole("Administrador");

        var isSupervisor =
            User.IsInRole("Supervisor") &&
            order.SupervisorId == userId;

        var isTechnician =
            User.IsInRole("Tecnico") &&
            order.TechnicianId == userId;

        if (!isAdmin &&
            !isSupervisor &&
            !isTechnician)
        {
            return NotFound();
        }

        var technician = order.TechnicianId == null
            ? null
            : await _users.FindByIdAsync(
                order.TechnicianId);

        var supervisor = order.SupervisorId == null
            ? null
            : await _users.FindByIdAsync(
                order.SupervisorId);

        var model = new WorkOrderDetailsViewModel
        {
            Id = order.Id,

            Title = order.Title,

            Description = order.Description,

            ServiceType = order.ServiceType,

            Status = order.Status,

            TechnicianName =
                technician?.FullName ?? "Sin asignar",

            SupervisorName =
                supervisor?.FullName ?? "Sin asignar",

            Diagnosis = order.Diagnosis,

            WorkPerformed = order.WorkPerformed,

            ReviewFeedback = order.ReviewFeedback,

            CanStart =
                isTechnician &&
                order.Status ==
                WorkOrderStatus.Assigned,

            CanSubmit =
                isTechnician &&
                order.Status ==
                WorkOrderStatus.InProgress,

            CanApprove =
                isSupervisor &&
                order.Status ==
                WorkOrderStatus.UnderReview,

            CanRequestChanges =
                isSupervisor &&
                order.Status ==
                WorkOrderStatus.UnderReview
        };

        return View(model);
    }

    // ==================================================
    // TÉCNICO: INICIAR TRABAJO
    // ==================================================

    [HttpPost]
    [Authorize(Roles = "Tecnico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == id &&
                x.TechnicianId == userId &&
                x.Status ==
                WorkOrderStatus.Assigned)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    WorkOrderStatus.InProgress));

        if (affected == 0)
            return NotFound();

        TempData["Success"] =
            "Intervención iniciada correctamente.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // ==================================================
    // TÉCNICO: ENVIAR INFORME A REVISIÓN
    // ==================================================

    [HttpPost]
    [Authorize(Roles = "Tecnico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitForReview(
        WorkOrderReportViewModel model)
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(userId))
            return Challenge();

        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(model.Diagnosis) ||
            string.IsNullOrWhiteSpace(model.WorkPerformed))
        {
            TempData["Error"] =
                "Debe completar el diagnóstico y " +
                "el trabajo realizado (máximo " +
                "4000 caracteres cada uno).";

            return RedirectToAction(
                nameof(Details),
                new { id = model.Id });
        }

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == model.Id &&
                x.TechnicianId == userId &&
                x.Status ==
                WorkOrderStatus.InProgress)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Diagnosis,
                    model.Diagnosis.Trim())
                .SetProperty(
                    x => x.WorkPerformed,
                    model.WorkPerformed.Trim())
                .SetProperty(
                    x => x.Status,
                    WorkOrderStatus.UnderReview)
                .SetProperty(
                    x => x.ReviewFeedback,
                    (string?)null));

        if (affected == 0)
            return NotFound();

        TempData["Success"] =
            "Informe enviado al supervisor para revisión.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }

    // ==================================================
    // SUPERVISOR: APROBAR Y CERRAR ORDEN
    // ==================================================

    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var supervisorId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(supervisorId))
            return Challenge();

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == id &&
                x.SupervisorId == supervisorId &&
                x.Status ==
                WorkOrderStatus.UnderReview &&
                x.TechnicianId != null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    WorkOrderStatus.Completed)
                .SetProperty(
                    x => x.CompletedAtUtc,
                    (DateTime?)DateTime.UtcNow)
                .SetProperty(
                    x => x.ReviewFeedback,
                    (string?)null));

        if (affected == 0)
            return NotFound();

        TempData["Success"] =
            "Orden aprobada y cerrada correctamente.";

        return RedirectToAction(
            nameof(Details),
            new { id });
    }

    // ==================================================
    // SUPERVISOR: SOLICITAR CORRECCIONES
    // ==================================================

    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestChanges(
        WorkOrderReviewViewModel model)
    {
        var supervisorId = _users.GetUserId(User);

        if (string.IsNullOrEmpty(supervisorId))
            return Challenge();

        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(model.Feedback))
        {
            TempData["Error"] =
                "Debe indicar las correcciones solicitadas " +
                "(máximo 2000 caracteres).";

            return RedirectToAction(
                nameof(Details),
                new { id = model.Id });
        }

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == model.Id &&
                x.SupervisorId == supervisorId &&
                x.Status ==
                WorkOrderStatus.UnderReview &&
                x.TechnicianId != null)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    WorkOrderStatus.InProgress)
                .SetProperty(
                    x => x.ReviewFeedback,
                    model.Feedback.Trim()));

        if (affected == 0)
            return NotFound();

        TempData["Success"] =
            "Orden devuelta al técnico para correcciones.";

        return RedirectToAction(
            nameof(Details),
            new { id = model.Id });
    }

    // ==================================================
    // LISTA DE TÉCNICOS SEGÚN EL ROL
    // ==================================================

    private async Task LoadTechnicians(
        WorkOrderCreateViewModel model)
    {
        var technicians = await _users
            .GetUsersInRoleAsync("Tecnico");

        var active = technicians
            .Where(x => x.IsActive);

        if (User.IsInRole("Supervisor"))
        {
            var supervisorId = _users.GetUserId(User);

            var assignedIds = await _db
                .SupervisorTechnicianAssignments
                .AsNoTracking()
                .Where(x =>
                    x.SupervisorId == supervisorId)
                .Select(x => x.TechnicianId)
                .ToListAsync();

            active = active.Where(
                x => assignedIds.Contains(x.Id));
        }

        model.Technicians = active
            .OrderBy(x => x.FullName)
            .Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.FullName
            })
            .ToList();
    }
}
