using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Domain.Entities;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor")]
public class EquipmentController : Controller
{
    private readonly ApplicationDbContext _db;

    public EquipmentController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status, int? clientId)
    {
        var query = _db.RefrigerationEquipments.AsNoTracking().Include(x => x.Client).AsQueryable();
        search = search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(term) || x.Client.Name.ToLower().Contains(term) || x.EquipmentType.ToLower().Contains(term) || (x.SerialNumber != null && x.SerialNumber.ToLower().Contains(term)) || (x.Brand != null && x.Brand.ToLower().Contains(term)));
        }
        if (status == "active") query = query.Where(x => x.IsActive && x.Client.IsActive);
        if (status == "inactive") query = query.Where(x => !x.IsActive || !x.Client.IsActive);
        if (clientId.HasValue) query = query.Where(x => x.ClientId == clientId.Value);
        ViewData["Search"] = search;
        ViewData["Status"] = status;
        ViewBag.ClientFilter = new SelectList(await _db.Clients.AsNoTracking().OrderBy(x => x.Name).ToListAsync(), "Id", "Name", clientId);
        var items = await query.OrderBy(x => x.Client.Name)
            .ThenBy(x => x.Name)
            .ToListAsync();

        return View(items);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new EquipmentCreateViewModel();
        await LoadClients(model);
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
            EquipmentCreateViewModel input)
    {
        var clientExists = await _db.Clients
            .AnyAsync(x => x.Id == input.ClientId && x.IsActive);

        if (!clientExists)
        {
            ModelState.AddModelError(
                nameof(input.ClientId),
                "Seleccione un cliente activo válido.");
        }

        if (!ModelState.IsValid)
        {
            await LoadClients(input);
            return View(input);
        }

        var equipment = new RefrigerationEquipment
        {
            ClientId = input.ClientId,
            Name = input.Name.Trim(),
            EquipmentType = input.EquipmentType.Trim(),
            Brand = input.Brand?.Trim(),
            Model = input.Model?.Trim(),
            SerialNumber = input.SerialNumber?.Trim(),
            Location = input.Location?.Trim(),
            RefrigerantType = input.RefrigerantType?.Trim(),
            Notes = input.Notes?.Trim(),
            IsActive = true
        };

        _db.RefrigerationEquipments.Add(equipment);
        await _db.SaveChangesAsync();

        TempData["Success"] =
                    "Equipo registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var equipment = await _db.RefrigerationEquipments.AsNoTracking().Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id);
        if (equipment == null) return NotFound();
        var orders = await _db.WorkOrders.AsNoTracking().Where(x => x.EquipmentId == id).OrderByDescending(x => x.CreatedAtUtc).ToListAsync();
        return View(new EquipmentDetailsViewModel { Equipment = equipment, Orders = orders });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var e = await _db.RefrigerationEquipments.AsNoTracking().Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        return View(new EquipmentEditViewModel { Id = e.Id, ClientId = e.ClientId, ClientName = e.Client.Name, Name = e.Name, EquipmentType = e.EquipmentType, Brand = e.Brand, Model = e.Model, SerialNumber = e.SerialNumber, Location = e.Location, RefrigerantType = e.RefrigerantType, Notes = e.Notes });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EquipmentEditViewModel input)
    {
        if (id != input.Id) return BadRequest();
        var e = await _db.RefrigerationEquipments.Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        input.ClientName = e.Client.Name;
        input.ClientId = e.ClientId;
        ModelState.Remove(nameof(input.ClientId));
        if (!ModelState.IsValid) return View(input);
        e.Name = input.Name.Trim();
        e.EquipmentType = input.EquipmentType.Trim();
        e.Brand = input.Brand?.Trim();
        e.Model = input.Model?.Trim();
        e.SerialNumber = input.SerialNumber?.Trim();
        e.Location = input.Location?.Trim();
        e.RefrigerantType = input.RefrigerantType?.Trim();
        e.Notes = input.Notes?.Trim();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Equipo actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool isActive)
    {
        if (!ModelState.IsValid || !Request.Form.ContainsKey(nameof(isActive))) return BadRequest();
        var e = await _db.RefrigerationEquipments.Include(x => x.Client).FirstOrDefaultAsync(x => x.Id == id);
        if (e == null) return NotFound();
        if (isActive && !e.Client.IsActive)
        {
            TempData["Error"] = "Active primero al cliente asociado.";
            return RedirectToAction(nameof(Details), new { id });
        }
        if (!isActive && await _db.WorkOrders.AnyAsync(x => x.EquipmentId == id && x.Status != RefrigeracionControl.Domain.Enums.WorkOrderStatus.Completed && x.Status != RefrigeracionControl.Domain.Enums.WorkOrderStatus.Cancelled))
        {
            TempData["Error"] = "No se puede desactivar un equipo con órdenes abiertas.";
            return RedirectToAction(nameof(Details), new { id });
        }
        e.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = "Estado actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task LoadClients(
            EquipmentCreateViewModel model)
    {
        model.Clients = await _db.Clients
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Name)
            .Select(x => new SelectListItem
            {
                Value = x.Id.ToString(),
                Text = x.Name
            })
            .ToListAsync();
    }
}