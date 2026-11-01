using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace RefrigeracionControl.Infrastructure.Identity;

public static class IdentitySeeder
{
	public static async Task SeedAsync(
	IServiceProvider services,
	IConfiguration configuration)
	{
		var roleManager = services
		.GetRequiredService<RoleManager<IdentityRole>>();

		var userManager = services
		.GetRequiredService<UserManager<ApplicationUser>>();

		string[] roles =
		{
"Administrador",
"Supervisor",
"Tecnico"
};

		foreach (var role in roles)
		{
			if (!await roleManager.RoleExistsAsync(role))
			{
				var result = await roleManager.CreateAsync(
				new IdentityRole(role));

				if (!result.Succeeded)
					throw new InvalidOperationException(
					string.Join("; ",
					result.Errors.Select(e => e.Description)));
			}
		}

		var email = configuration["BootstrapAdmin:Email"];
		var password = configuration["BootstrapAdmin:Password"];

		if (string.IsNullOrWhiteSpace(email) ||
		string.IsNullOrWhiteSpace(password))
		{
			return;
		}

		var admin = await userManager.FindByEmailAsync(email);

		if (admin == null)
		{
			admin = new ApplicationUser
			{
				UserName = email,
				Email = email,
				EmailConfirmed = true,
				FullName = "Administrador principal",
				IsActive = true
			};

			var result = await userManager.CreateAsync(
			admin, password);

			if (!result.Succeeded)
				throw new InvalidOperationException(
				string.Join("; ",
				result.Errors.Select(e => e.Description)));
		}

		if (!await userManager.IsInRoleAsync(
		admin, "Administrador"))
		{
			var result = await userManager.AddToRoleAsync(
			admin, "Administrador");

			if (!result.Succeeded)
				throw new InvalidOperationException(
				string.Join("; ",
				result.Errors.Select(e => e.Description)));
		}
	}
}