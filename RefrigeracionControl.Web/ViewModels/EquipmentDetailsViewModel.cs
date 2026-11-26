using RefrigeracionControl.Domain.Entities;
namespace RefrigeracionControl.Web.ViewModels;
public class EquipmentDetailsViewModel
{
    public RefrigerationEquipment Equipment { get; set; } = null!;
    public List<WorkOrder> Orders { get; set; } = new();
}
