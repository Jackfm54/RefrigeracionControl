
using System.ComponentModel.DataAnnotations;

namespace RefrigeracionControl.Web.ViewModels;

public class CreateUserViewModel
{
	[Required(ErrorMessage = "Ingrese el nombre.")]
	[StringLength(120)]
	public string FullName { get; set; } = "";

	[Required]
	[EmailAddress]
	public string Email { get; set; } = "";

	[Required]
	[MinLength(12)]
	[DataType(DataType.Password)]
	public string Password { get; set; } = "";

	[Required]
	[Compare(nameof(Password),
		ErrorMessage = "Las contraseñas no coinciden.")]
	[DataType(DataType.Password)]
	public string ConfirmPassword { get; set; } = "";

	[Required]
	public string Role { get; set; } = "Tecnico";
}
