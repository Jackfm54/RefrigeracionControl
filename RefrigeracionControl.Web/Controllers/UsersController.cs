
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

[Authorize(Roles = "Administrador")]
public class UsersController : Controller
{
	private readonly UserManager<ApplicationUser> _users;

	public UsersController(UserManager<ApplicationUser> users)
	{
		_users = users;
	}

	[HttpGet]
	public async Task<IActionResult> Index()
	{
		var users = await _users.Users
			.AsNoTracking()
			.OrderBy(u => u.FullName)
			.ToListAsync();

		var result = new List<UserListViewModel>();

		foreach (var user in users)
		{
			var roles = await _users.GetRolesAsync(user);

			result.Add(new UserListViewModel
			{
				Id = user.Id,
				FullName = user.FullName,
				Email = user.Email ?? "",
				Role = string.Join(", ", roles),
				IsActive = user.IsActive,
				IsAdministrator = roles.Contains("Administrador")
			});
		}

		return View(result);
	}

	[HttpGet]
	public IActionResult Create()
	{
		return View(new CreateUserViewModel());
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Create(
		CreateUserViewModel model)
	{
		if (model.Role != "Tecnico" &&
			model.Role != "Supervisor")
		{
			ModelState.AddModelError(
				nameof(model.Role),
				"Seleccione un rol permitido.");
		}

		if (!ModelState.IsValid)
			return View(model);

		var email = model.Email.Trim();

		if (await _users.FindByEmailAsync(email) != null)
		{
			ModelState.AddModelError(
				nameof(model.Email),
				"Ya existe una cuenta con este correo.");
			return View(model);
		}

		var user = new ApplicationUser
		{
			UserName = email,
			Email = email,
			FullName = model.FullName.Trim(),
			IsActive = true,
			EmailConfirmed = false
		};

		var created = await _users.CreateAsync(
			user, model.Password);

		if (!created.Succeeded)
		{
			foreach (var error in created.Errors)
				ModelState.AddModelError(
					"", error.Description);

			return View(model);
		}

		var roleResult = await _users.AddToRoleAsync(
			user, model.Role);

		if (!roleResult.Succeeded)
		{
			// No dejar cuentas sin rol en esta operación
			var rollback = await _users.DeleteAsync(user);

			if (!rollback.Succeeded)
				throw new InvalidOperationException(
					"No se pudo revertir la creación del usuario.");

			ModelState.AddModelError(
				"", "No fue posible asignar el rol.");

			return View(model);
		}

		TempData["Success"] = "Usuario creado correctamente.";

		return RedirectToAction(nameof(Index));
	}

	[HttpPost]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Deactivate(string id)
	{
		var user = await _users.FindByIdAsync(id);

		if (user == null)
			return NotFound();

		if (await _users.IsInRoleAsync(
			user, "Administrador"))
			return Forbid();

		if (!user.IsActive)
			return RedirectToAction(nameof(Index));

		user.IsActive = false;

		var updated = await _users.UpdateAsync(user);

		if (!updated.Succeeded)
		{
			TempData["Error"] = "No se pudo desactivar la cuenta.";
			return RedirectToAction(nameof(Index));
		}

		// Invalidar las cookies existentes
		var stamp = await _users.UpdateSecurityStampAsync(user);

		if (!stamp.Succeeded)
			throw new InvalidOperationException(
				"No se pudo invalidar la sesión del usuario.");

		TempData["Success"] = "Usuario desactivado.";

		return RedirectToAction(nameof(Index));
	}
}
