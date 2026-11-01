
using RefrigeracionControl.Domain.Enums;

namespace RefrigeracionControl.Domain.Entities;

public class WorkOrder
{
    public int Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ServiceType { get; set; } = string.Empty;

    public WorkOrderStatus Status { get; set; }
        = WorkOrderStatus.Pending;

    public string? SupervisorId { get; set; }

    public string? TechnicianId { get; set; }

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public DateTime? ScheduledAtUtc { get; set; }

    public DateTime? CompletedAtUtc { get; set; }

    public string? Diagnosis { get; set; }

    public string? WorkPerformed { get; set; }

    public string? ReviewFeedback { get; set; }

}
