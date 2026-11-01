
using System.ComponentModel.DataAnnotations;
using RefrigeracionControl.Domain.Enums;

namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderDetailsViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string ServiceType { get; set; } = "";

    public WorkOrderStatus Status { get; set; }

    public string TechnicianName { get; set; } = "";
    public string SupervisorName { get; set; } = "";

    public string? Diagnosis { get; set; }
    public string? WorkPerformed { get; set; }

    public bool CanStart { get; set; }
    public bool CanSubmit { get; set; }
}

public class WorkOrderReportViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Ingrese el diagnóstico.")]
    [StringLength(4000)]
    public string Diagnosis { get; set; } = "";

    [Required(ErrorMessage = "Describa el trabajo realizado.")]
    [StringLength(4000)]
    public string WorkPerformed { get; set; } = "";
}
