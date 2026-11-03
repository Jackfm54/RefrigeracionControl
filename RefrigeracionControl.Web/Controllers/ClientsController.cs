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
    public async Task<IActionResult> Index()
    {
        var clients = await _db.Clients
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ToListAsync();

        return View(clients);
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
}