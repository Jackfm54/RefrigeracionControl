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
    public async Task<IActionResult> Index()
    {
        var items = await _db.RefrigerationEquipments
            .AsNoTracking()
            .Include(x => x.Client)
            .OrderBy(x => x.Client.Name)
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