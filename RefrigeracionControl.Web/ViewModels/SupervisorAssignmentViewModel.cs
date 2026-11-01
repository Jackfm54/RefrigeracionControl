
using System.ComponentModel.DataAnnotations;

namespace RefrigeracionControl.Web.ViewModels;

public class SupervisorAssignmentViewModel
{
    [Required(ErrorMessage = "Seleccione un supervisor.")]
    public string SupervisorId { get; set; } = "";

    [Required(ErrorMessage = "Seleccione un técnico.")]
    public string TechnicianId { get; set; } = "";

    public List<AssignmentOption> Supervisors { get; set; } = new();

    public List<AssignmentOption> Technicians { get; set; } = new();
}

public class AssignmentOption
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
}
