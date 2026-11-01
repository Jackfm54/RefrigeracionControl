
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderCreateViewModel
{
    [Required(ErrorMessage = "Ingrese el título.")]
    [StringLength(180)]
    public string Title { get; set; } = "";

    [Required(ErrorMessage = "Ingrese la descripción.")]
    [StringLength(4000)]
    public string Description { get; set; } = "";

    [Required]
    public string ServiceType { get; set; } = "Preventivo";

    public string? TechnicianId { get; set; }

    public DateTime? ScheduledAt { get; set; }

    public List<SelectListItem> Technicians { get; set; } = new();
}
