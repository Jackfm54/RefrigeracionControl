
namespace RefrigeracionControl.Domain.Entities;

public class Client
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? ContactName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? City { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;

    public ICollection<RefrigerationEquipment> Equipments
    { get; set; } = new List<RefrigerationEquipment>();
}
