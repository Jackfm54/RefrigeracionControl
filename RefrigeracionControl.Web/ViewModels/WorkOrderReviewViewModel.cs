
using System.ComponentModel.DataAnnotations;

namespace RefrigeracionControl.Web.ViewModels;

public class WorkOrderReviewViewModel
{
    [Range(1, int.MaxValue)]
    public int Id { get; set; }

    [Required(
        ErrorMessage = "Indique las correcciones solicitadas.")]
    [StringLength(
        2000,
        ErrorMessage = "Máximo 2000 caracteres.")]
    public string Feedback { get; set; } = string.Empty;
}
