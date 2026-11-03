using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace RefrigeracionControl.Web.ViewModels;

public class EquipmentCreateViewModel
{
    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione un cliente.")]
    public int ClientId { get; set; }

    [Required(ErrorMessage = "Ingrese el nombre del equipo.")]
    [StringLength(180)]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "Ingrese el tipo de equipo.")]
    [StringLength(100)]
    public string EquipmentType { get; set; } = "";


    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(150)]
    public string? SerialNumber { get; set; }

    [StringLength(200)]
    public string? Location { get; set; }

    [StringLength(80)]
    public string? RefrigerantType { get; set; }

    [StringLength(2000)]
    public string? Notes { get; set; }

    [ValidateNever]
    public List<SelectListItem> Clients { get; set; } = new();
}