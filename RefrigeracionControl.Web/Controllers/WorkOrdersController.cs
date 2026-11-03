
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

    // =============================================
    // LISTADO DE ÓRDENES
    // =============================================
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var query = _db.WorkOrders.AsNoTracking();

        if (User.IsInRole("Administrador"))
        {
            // El administrador consulta todas las órdenes.
        }
        else if (User.IsInRole("Supervisor"))
        {
            query = query.Where(x =>
                x.SupervisorId == userId);
        }
        else if (User.IsInRole("Tecnico"))
        {
            query = query.Where(x =>
                x.TechnicianId == userId);
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

    // =============================================
    // NUEVA ORDEN - FORMULARIO
    // =============================================
    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> Create()
    {
        var input = new WorkOrderCreateViewModel();

        await LoadCreateOptions(input);

        return View(input);
    }

    // =============================================
    // NUEVA ORDEN - GUARDAR
    // =============================================
    [HttpPost]
    [Authorize(Roles = "Administrador,Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
        WorkOrderCreateViewModel input)
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (input.ServiceType != "Preventivo" &&
            input.ServiceType != "Correctivo")
        {
            ModelState.AddModelError(
                nameof(input.ServiceType),
                "Tipo de mantenimiento no permitido.");
        }

        var validClient = await _db.Clients
            .AsNoTracking()
            .AnyAsync(x =>
                x.Id == input.ClientId &&
                x.IsActive);

        if (!validClient)
        {
            ModelState.AddModelError(
                nameof(input.ClientId),
                "Seleccione un cliente activo válido.");
        }

        var validEquipment =
            await _db.RefrigerationEquipments
                .AsNoTracking()
                .AnyAsync(x =>
                    x.Id == input.EquipmentId &&
                    x.ClientId == input.ClientId &&
                    x.IsActive &&
                    x.Client.IsActive);

        if (!validEquipment)
        {
            ModelState.AddModelError(
                nameof(input.EquipmentId),
                "El equipo no pertenece al cliente " +
                "seleccionado o está inactivo.");
        }

        string? supervisorId =
            User.IsInRole("Supervisor")
                ? userId
                : null;

        if (!string.IsNullOrWhiteSpace(input.TechnicianId))
        {
            var technician = await _users.FindByIdAsync(
                input.TechnicianId);

            if (technician == null ||
                !technician.IsActive ||
                !await _users.IsInRoleAsync(
                    technician, "Tecnico"))
            {
                ModelState.AddModelError(
                    nameof(input.TechnicianId),
                    "Seleccione un técnico activo válido.");
            }
            else
            {
                var assignment = await _db
                    .SupervisorTechnicianAssignments
                    .AsNoTracking()
                    .SingleOrDefaultAsync(x =>
                        x.TechnicianId == input.TechnicianId);

                if (User.IsInRole("Supervisor"))
                {
                    if (assignment?.SupervisorId != userId)
                    {
                        ModelState.AddModelError(
                            nameof(input.TechnicianId),
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
            await LoadCreateOptions(input);
            return View(input);
        }

        var order = new WorkOrder
        {
            Title = input.Title.Trim(),
            Description = input.Description.Trim(),
            ServiceType = input.ServiceType,

            ClientId = input.ClientId,
            EquipmentId = input.EquipmentId,

            TechnicianId = string.IsNullOrWhiteSpace(
                input.TechnicianId)
                ? null
                : input.TechnicianId,

            SupervisorId = supervisorId,

            Status = string.IsNullOrWhiteSpace(
                input.TechnicianId)
                ? WorkOrderStatus.Pending
                : WorkOrderStatus.Assigned,

            CreatedAtUtc = DateTime.UtcNow,

            // Se conserva la conversión local usada
            // anteriormente. Antes de producción se
            // configurará la zona horaria de la empresa.
            ScheduledAtUtc = input.ScheduledAt.HasValue
                ? DateTime.SpecifyKind(
                    input.ScheduledAt.Value,
                    DateTimeKind.Local).ToUniversalTime()
                : null
        };

        _db.WorkOrders.Add(order);
        await _db.SaveChangesAsync();

        TempData["Success"] =
            "Orden de servicio registrada correctamente.";

        return RedirectToAction(nameof(Index));
    }

    // =============================================
    // EQUIPOS DE UN CLIENTE - AJAX
    // =============================================
    [HttpGet]
    [Authorize(Roles = "Administrador,Supervisor")]
    public async Task<IActionResult> GetEquipments(int clientId)
    {
        if (clientId <= 0)
        {
            return Json(Array.Empty<object>());
        }

        var equipments = await _db.RefrigerationEquipments
            .AsNoTracking()
            .Where(x =>
                x.ClientId == clientId &&
                x.IsActive &&
                x.Client.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new
            {
                id = x.Id,
                name = x.Name + " - " + x.EquipmentType
            })
            .ToListAsync();

        return Json(equipments);
    }

    // =============================================
    // DETALLE DE LA ORDEN
    // =============================================
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var order = await _db.WorkOrders
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id);

        if (order == null)
            return NotFound();

        var userId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var isAdmin = User.IsInRole("Administrador");

        var isSupervisor =
            User.IsInRole("Supervisor") &&
            order.SupervisorId == userId;

        var isTechnician =
            User.IsInRole("Tecnico") &&
            order.TechnicianId == userId;

        if (!isAdmin && !isSupervisor && !isTechnician)
            return NotFound();

        var technician = order.TechnicianId == null
            ? null
            : await _users.FindByIdAsync(
                order.TechnicianId);

        var supervisor = order.SupervisorId == null
            ? null
            : await _users.FindByIdAsync(
                order.SupervisorId);

        var client = order.ClientId.HasValue
            ? await _db.Clients
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == order.ClientId.Value)
            : null;

        var equipment = order.EquipmentId.HasValue
            ? await _db.RefrigerationEquipments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == order.EquipmentId.Value)
            : null;

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

            ClientName =
                client?.Name ?? "No registrado",

            EquipmentName =
                equipment?.Name ?? "No registrado",

            EquipmentSerialNumber =
                equipment?.SerialNumber ?? "",

            Diagnosis = order.Diagnosis,
            WorkPerformed = order.WorkPerformed,
            ReviewFeedback = order.ReviewFeedback,

            CanStart = isTechnician &&
                order.Status == WorkOrderStatus.Assigned,

            CanSubmit = isTechnician &&
                order.Status == WorkOrderStatus.InProgress,

            CanApprove = isSupervisor &&
                order.Status == WorkOrderStatus.UnderReview,

            CanRequestChanges = isSupervisor &&
                order.Status == WorkOrderStatus.UnderReview
        };

        return View(model);
    }

    // =============================================
    // TÉCNICO - INICIAR INTERVENCIÓN
    // =============================================
    [HttpPost]
    [Authorize(Roles = "Tecnico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Start(int id)
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == id &&
                x.TechnicianId == userId &&
                x.Status == WorkOrderStatus.Assigned)
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

    // =============================================
    // TÉCNICO - ENVIAR INFORME
    // =============================================
    [HttpPost]
    [Authorize(Roles = "Tecnico")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SubmitForReview(
        WorkOrderReportViewModel input)
    {
        var userId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(userId))
            return Challenge();

        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(input.Diagnosis) ||
            string.IsNullOrWhiteSpace(input.WorkPerformed))
        {
            TempData["Error"] =
                "Complete el diagnóstico y el trabajo " +
                "realizado (máximo 4000 caracteres cada uno).";

            return RedirectToAction(
                nameof(Details),
                new { id = input.Id });
        }

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == input.Id &&
                x.TechnicianId == userId &&
                x.Status == WorkOrderStatus.InProgress)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Diagnosis,
                    input.Diagnosis.Trim())
                .SetProperty(
                    x => x.WorkPerformed,
                    input.WorkPerformed.Trim())
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
            new { id = input.Id });
    }

    // =============================================
    // SUPERVISOR - APROBAR Y CERRAR
    // =============================================
    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var supervisorId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(supervisorId))
            return Challenge();

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == id &&
                x.SupervisorId == supervisorId &&
                x.TechnicianId != null &&
                x.Status == WorkOrderStatus.UnderReview)
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

    // =============================================
    // SUPERVISOR - SOLICITAR CORRECCIONES
    // =============================================
    [HttpPost]
    [Authorize(Roles = "Supervisor")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RequestChanges(
        WorkOrderReviewViewModel input)
    {
        var supervisorId = _users.GetUserId(User);

        if (string.IsNullOrWhiteSpace(supervisorId))
            return Challenge();

        if (!ModelState.IsValid ||
            string.IsNullOrWhiteSpace(input.Feedback))
        {
            TempData["Error"] =
                "Ingrese las correcciones solicitadas " +
                "(máximo 2000 caracteres).";

            return RedirectToAction(
                nameof(Details),
                new { id = input.Id });
        }

        var affected = await _db.WorkOrders
            .Where(x =>
                x.Id == input.Id &&
                x.SupervisorId == supervisorId &&
                x.TechnicianId != null &&
                x.Status == WorkOrderStatus.UnderReview)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(
                    x => x.Status,
                    WorkOrderStatus.InProgress)
                .SetProperty(
                    x => x.ReviewFeedback,
                    input.Feedback.Trim()));

        if (affected == 0)
            return NotFound();

        TempData["Success"] =
            "Orden devuelta al técnico para correcciones.";

        return RedirectToAction(
            nameof(Details),
            new { id = input.Id });
    }

    // =============================================
    // CARGAR TÉCNICOS SEGÚN EL ROL
    // =============================================
    private async Task LoadTechnicians(
        WorkOrderCreateViewModel input)
    {
        var technicians = await _users
            .GetUsersInRoleAsync("Tecnico");

        var activeTechnicians = technicians
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

            activeTechnicians = activeTechnicians
                .Where(x => assignedIds.Contains(x.Id));
        }

        input.Technicians = activeTechnicians
            .OrderBy(x => x.FullName)
            .Select(x => new SelectListItem
            {
                Value = x.Id,
                Text = x.FullName
            })
            .ToList();
    }

    // =============================================
    // CARGAR TODAS LAS OPCIONES DEL FORMULARIO
    // =============================================
    private async Task LoadCreateOptions(
        WorkOrderCreateViewModel input)
    {
        await LoadTechnicians(input);

        input.Clients = await _db.Clients
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();

        // Solo cargar los equipos del cliente elegido.
        // Al abrir el formulario, la selección está vacía.
        input.Equipments = await _db
            .RefrigerationEquipments
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Client.IsActive &&
                x.ClientId == input.ClientId)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name + " - " + x.EquipmentType
            })
            .ToListAsync();

        // Diagnóstico temporal: retirar tras las pruebas.
        Console.WriteLine(
            $"Clientes cargados: {input.Clients.Count}");
        Console.WriteLine(
            $"Equipos del cliente cargados: {input.Equipments.Count}");
        Console.WriteLine(
            $"Técnicos cargados: {input.Technicians.Count}");
    }
}
