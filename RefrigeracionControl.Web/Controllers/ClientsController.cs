using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Domain.Entities;
using RefrigeracionControl.Infrastructure.Data;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador,Supervisor")]
public class ClientsController : Controller
{
    private readonly ApplicationDbContext _db;

    public ClientsController(ApplicationDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Index(string? search, string? status)
    {
        var query = _db.Clients.AsNoTracking();
        search = search?.Trim();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(term)
                || (x.ContactName != null && x.ContactName.ToLower().Contains(term))
                || (x.City != null && x.City.ToLower().Contains(term))
                || (x.Email != null && x.Email.ToLower().Contains(term))
                || (x.Phone != null && x.Phone.Contains(term)));
        }
        if (status == "active") query = query.Where(x => x.IsActive);
        else if (status == "inactive") query = query.Where(x => !x.IsActive);
        ViewData["Search"] = search;
        ViewData["Status"] = status;
        return View(await query.OrderBy(x => x.Name).ToListAsync());
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ClientCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(
            ClientCreateViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var client = new Client
        {
            Name = model.Name.Trim(),
            ContactName = model.ContactName?.Trim(),
            Email = model.Email?.Trim(),
            Phone = model.Phone?.Trim(),
            Address = model.Address?.Trim(),
            City = model.City?.Trim(),
            Notes = model.Notes?.Trim(),
            IsActive = true
        };

        _db.Clients.Add(client);
        await _db.SaveChangesAsync();

        TempData["Success"] =
                    "Cliente registrado correctamente.";

        return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Details(int id)
    {
        var client = await _db.Clients.AsNoTracking()
            .Include(x => x.Equipments)
            .SingleOrDefaultAsync(x => x.Id == id);
        return client == null ? NotFound() : View(client);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var client = await _db.Clients.AsNoTracking().SingleOrDefaultAsync(x => x.Id == id);
        if (client == null) return NotFound();
        return View(new ClientEditViewModel
        {
            Id = client.Id, Name = client.Name, ContactName = client.ContactName,
            Email = client.Email, Phone = client.Phone, Address = client.Address,
            City = client.City, Notes = client.Notes
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClientEditViewModel model)
    {
        if (id != model.Id) return BadRequest();
        var client = await _db.Clients.FindAsync(id);
        if (client == null) return NotFound();
        if (!ModelState.IsValid) return View(model);
        client.Name = model.Name.Trim();
        client.ContactName = model.ContactName?.Trim();
        client.Email = model.Email?.Trim();
        client.Phone = model.Phone?.Trim();
        client.Address = model.Address?.Trim();
        client.City = model.City?.Trim();
        client.Notes = model.Notes?.Trim();
        await _db.SaveChangesAsync();
        TempData["Success"] = "Cliente actualizado correctamente.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int id, bool isActive)
    {
        var client = await _db.Clients.FindAsync(id);
        if (client == null) return NotFound();
        client.IsActive = isActive;
        await _db.SaveChangesAsync();
        TempData["Success"] = isActive ? "Cliente activado." : "Cliente desactivado.";
        return RedirectToAction(nameof(Details), new { id });
    }
}
