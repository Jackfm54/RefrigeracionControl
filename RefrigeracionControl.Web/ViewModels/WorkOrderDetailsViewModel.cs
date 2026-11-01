
using System.ComponentModel.DataAnnotations;
using RefrigeracionControl.Domain.Enums;

namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public WorkOrderStatus Status { get; set; }

    public string TechnicianName { get; set; } = string.Empty;

    public string SupervisorName { get; set; } = string.Empty;

    public string? Diagnosis { get; set; }

    public string? WorkPerformed { get; set; }

    // Observaciones realizadas por el supervisor.
    public string? ReviewFeedback { get; set; }

    // Permisos para el técnico.
    public bool CanStart { get; set; }

    public bool CanSubmit { get; set; }

    // Permisos para el supervisor responsable.
    public bool CanApprove { get; set; }

    public bool CanRequestChanges { get; set; }
}

public class WorkOrderReportViewModel
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el diagnóstico.")]
    [StringLength(4000)]
    public string Diagnosis { get; set; } = string.Empty;

    [Required(ErrorMessage = "Describa el trabajo realizado.")]
    [StringLength(4000)]
    public string WorkPerformed { get; set; } = string.Empty;
}
