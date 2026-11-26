using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace RefrigeracionControl.Web.ViewModels;
public class EquipmentEditViewModel : EquipmentCreateViewModel
{
    public int Id { get; set; }
    [ValidateNever]
    public string ClientName { get; set; } = "";
}
