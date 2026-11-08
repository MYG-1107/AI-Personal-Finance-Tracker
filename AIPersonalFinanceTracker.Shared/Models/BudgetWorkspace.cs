namespace AIPersonalFinanceTracker.Shared.Models;

public class BudgetWorkspace
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<BudgetMember> Members { get; set; } = new();
}

public class BudgetMember
{
    public int Id { get; set; }
    public int BudgetWorkspaceId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}

public class CreateWorkspaceRequest
{
    public string Name { get; set; } = string.Empty;
    public string OwnerEmail { get; set; } = string.Empty;
}

public class InviteWorkspaceMemberRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer";
}