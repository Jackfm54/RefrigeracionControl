
namespace RefrigeracionControl.Domain.Entities;

public class RefrigerationEquipment
{
    public int Id { get; set; }

    public int ClientId { get; set; }

    public Client Client { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string EquipmentType { get; set; } = string.Empty;

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public string? Location { get; set; }

    public string? RefrigerantType { get; set; }

    public string? Notes { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAtUtc { get; set; }
        = DateTime.UtcNow;
}
