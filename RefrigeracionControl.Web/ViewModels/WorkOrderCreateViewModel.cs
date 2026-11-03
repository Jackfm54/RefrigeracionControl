
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderCreateViewModel
{
    [Required(ErrorMessage = "Ingrese el título de la orden.")]
    [StringLength(180)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Ingrese la descripción.")]
    [StringLength(4000)]
    public string Description { get; set; } = "";

    [Required]
    public string ServiceType { get; set; } = "Preventivo";

    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione un cliente.")]
    public int ClientId { get; set; }

    [Range(1, int.MaxValue,
        ErrorMessage = "Seleccione un equipo.")]
    public int EquipmentId { get; set; }

    public string? TechnicianId { get; set; }

    public DateTime? ScheduledAt { get; set; }

    [ValidateNever]
    public List<SelectListItem> Clients { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Equipments { get; set; } = new();

    [ValidateNever]
    public List<SelectListItem> Technicians { get; set; } = new();
}
