
namespace RefrigeracionControl.Web.ViewModels;

public class DashboardViewModel
{
	public int TotalOrders { get; set; }

	public int PendingOrders { get; set; }

	public int AssignedOrders { get; set; }

	public int InProgressOrders { get; set; }

	public int UnderReviewOrders { get; set; }

	public int CompletedOrders { get; set; }

	public int CancelledOrders { get; set; }

	public int ActiveTechnicians { get; set; }

	public int ActiveSupervisors { get; set; }

	public int TotalUsers { get; set; }

	public string DashboardScope { get; set; } = "";
}
