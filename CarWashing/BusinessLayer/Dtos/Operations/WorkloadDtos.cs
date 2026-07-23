namespace BusinessLayer.Dtos.Operations;

public class StaffWorkloadResponse
{
    public Guid StaffUserId { get; set; }
    public string StaffName { get; set; } = string.Empty;
    public int VehiclesParticipated { get; set; }
    public int VehiclesCompleted { get; set; }
    public decimal EquivalentVehicles { get; set; }
    public decimal EquivalentRevenue { get; set; }
}
