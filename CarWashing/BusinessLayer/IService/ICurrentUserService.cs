namespace BusinessLayer.IService
{
    public interface ICurrentUserService
    {
        Guid? UserId { get; }
        string? Username { get; }
        string? Email { get; }
        string? Role { get; }
        string? FullName { get; }
        Guid? CustomerId { get; }
        bool IsAuthenticated { get; }
        bool IsInRole(string role);
    }
}
