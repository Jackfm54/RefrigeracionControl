
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using RefrigeracionControl.Infrastructure.Identity;
using RefrigeracionControl.Web.ViewModels;

namespace RefrigeracionControl.Web.Controllers;

public class AccountController : Controller
{
	private readonly UserManager<ApplicationUser> _users;
	private readonly SignInManager<ApplicationUser> _signIn;

	public AccountController(
		UserManager<ApplicationUser> users,
		SignInManager<ApplicationUser> signIn)
	{
		_users = users;
		_signIn = signIn;
	}

	[HttpGet]
	[AllowAnonymous]
	public IActionResult Login()
	{
		if (User.Identity?.IsAuthenticated == true)
			return RedirectToAction("Index", "Dashboard");

		return View(new LoginViewModel());
	}

	[HttpPost]
	[AllowAnonymous]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Login(
		LoginViewModel model)
	{
		if (!ModelState.IsValid)
			return View(model);

		var user = await _users.FindByEmailAsync(model.Email);

		if (user == null || !user.IsActive)
		{
			ModelState.AddModelError(
				"", "Credenciales inválidas.");
			return View(model);
		}

		var result = await _signIn.PasswordSignInAsync(
			user,
			model.Password,
			model.RememberMe,
			lockoutOnFailure: true);

		if (result.Succeeded)
			return RedirectToAction("Index", "Dashboard");

		if (result.IsLockedOut)
		{
			ModelState.AddModelError(
				"", "Cuenta bloqueada temporalmente.");
			return View(model);
		}

		ModelState.AddModelError(
			"", "Credenciales inválidas.");

		return View(model);
	}

	[HttpPost]
	[Authorize]
	[ValidateAntiForgeryToken]
	public async Task<IActionResult> Logout()
	{
		await _signIn.SignOutAsync();
		return RedirectToAction("Login");
	}

	[HttpGet]
	public IActionResult AccessDenied()
	{
		return View();
	}
}
