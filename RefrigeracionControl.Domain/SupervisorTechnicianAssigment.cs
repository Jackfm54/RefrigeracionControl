
namespace RefrigeracionControl.Domain.Entities;

public class SupervisorTechnicianAssignment
{
    public int Id { get; set; }

    public string SupervisorId { get; set; } = string.Empty;

    public string TechnicianId { get; set; } = string.Empty;

    public DateTime AssignedAtUtc { get; set; } =
        DateTime.UtcNow;
}
