
using Microsoft.AspNetCore.Identity;

namespace RefrigeracionControl.Infrastructure.Identity;

public class ApplicationUser : IdentityUser
{
	public string FullName { get; set; } = string.Empty;

	public bool IsActive { get; set; } = true;

	public DateTime CreatedAtUtc { get; set; }
		= DateTime.UtcNow;
}
