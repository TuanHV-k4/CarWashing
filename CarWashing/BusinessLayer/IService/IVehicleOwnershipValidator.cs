namespace BusinessLayer.IService
{
    public interface IVehicleOwnershipValidator
    {
        Task<bool> IsOwnedByCustomerAsync(Guid vehicleId, Guid customerId);
        Task EnsureOwnedByCustomerAsync(Guid vehicleId, Guid customerId);
    }
}
