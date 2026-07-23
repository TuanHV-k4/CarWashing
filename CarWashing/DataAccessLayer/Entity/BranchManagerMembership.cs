namespace DataAccessLayer.Entity;
public class BranchManagerMembership { public Guid BranchManagerMembershipID { get; set; } = Guid.NewGuid(); public Guid BranchID { get; set; } public Guid UserID { get; set; } public bool IsActive { get; set; } = true; public Branch Branch { get; set; } = null!; public User User { get; set; } = null!; }
