
using System.ComponentModel.DataAnnotations;

namespace RefrigeracionControl.Web.ViewModels;

public class LoginViewModel
{
	[Required(ErrorMessage = "Ingrese su correo.")]
	[EmailAddress]
	public string Email { get; set; } = string.Empty;

	[Required(ErrorMessage = "Ingrese su contraseña.")]
	[DataType(DataType.Password)]
	public string Password { get; set; } = string.Empty;

	public bool RememberMe { get; set; }
}
