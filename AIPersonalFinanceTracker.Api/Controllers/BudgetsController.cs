using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BudgetsController : ControllerBase
{
    private readonly AppDbContext _context;

    public BudgetsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<BudgetWorkspace>>> GetWorkspaces([FromQuery] string? email)
    {
        var workspaces = await _context.BudgetWorkspaces
            .Include(workspace => workspace.Members)
            .Where(workspace => string.IsNullOrWhiteSpace(email) ||
                workspace.OwnerEmail == email || workspace.Members.Any(member => member.Email == email))
            .AsNoTracking()
            .ToListAsync();
        return Ok(workspaces);
    }

    [HttpPost]
    public async Task<ActionResult<BudgetWorkspace>> Create(CreateWorkspaceRequest request)
    {
        if (request is null || string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.OwnerEmail))
            return BadRequest(new { message = "Workspace name and owner email are required." });

        var workspace = new BudgetWorkspace
        {
            Name = request.Name.Trim(),
            OwnerEmail = request.OwnerEmail.Trim().ToLowerInvariant()
        };
        workspace.Members.Add(new BudgetMember { Email = workspace.OwnerEmail, Role = "Owner" });
        _context.BudgetWorkspaces.Add(workspace);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = workspace.Id }, workspace);
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<BudgetWorkspace>> GetById(int id)
    {
        var workspace = await _context.BudgetWorkspaces
            .Include(item => item.Members)
            .AsNoTracking()
            .SingleOrDefaultAsync(item => item.Id == id);
        return workspace is null ? NotFound() : Ok(workspace);
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<BudgetMember>> Invite(int id, InviteWorkspaceMemberRequest request)
    {
        var workspace = await _context.BudgetWorkspaces.FindAsync(id);
        if (workspace is null)
            return NotFound();
        if (request is null || string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Member email is required." });

        var role = request.Role.Trim();
        if (!string.Equals(role, "Viewer", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(role, "Contributor", StringComparison.OrdinalIgnoreCase))
            return BadRequest(new { message = "Role must be Viewer or Contributor." });

        var member = new BudgetMember
        {
            BudgetWorkspaceId = id,
            Email = request.Email.Trim().ToLowerInvariant(),
            Role = char.ToUpperInvariant(role[0]) + role[1..].ToLowerInvariant()
        };
        _context.BudgetMembers.Add(member);
        await _context.SaveChangesAsync();
        return Ok(member);
    }
}