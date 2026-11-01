
namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderListViewModel
{
    public int Id { get; set; }

    public string Title { get; set; } = "";

    public string ServiceType { get; set; } = "";

    public string Status { get; set; } = "";

    public string TechnicianName { get; set; } = "";

    public DateTime CreatedAtUtc { get; set; }
}
