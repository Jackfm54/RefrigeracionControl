
namespace RefrigeracionControl.Web.ViewModels;

public class UserListViewModel
{
    public string Id { get; set; } = "";
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
    public bool IsAdministrator { get; set; }
}

