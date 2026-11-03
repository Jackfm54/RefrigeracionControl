using System.ComponentModel.DataAnnotations;

namespace RefrigeracionControl.Web.ViewModels;

public class ClientCreateViewModel
{
    [Required(ErrorMessage = "Ingrese el nombre del cliente.")]
    [StringLength(200)]
    public string Name { get; set; } = "";

    [StringLength(150)]
    public string? ContactName { get; set; }

    [EmailAddress]
    [StringLength(254)]
    public string? Email { get; set; }

    [Phone]
    [StringLength(40)]
    public string? Phone { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }

    [StringLength(120)]
    public string? City { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }
}