using AIPersonalFinanceTracker.Api.Controllers;
using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Tests;

public class BudgetsControllerTests
{
    [Fact]
    public async Task CreateAndInvite_StoresWorkspaceRoles()
    {
        var context = new AppDbContext(new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString()).Options);
        var controller = new BudgetsController(context);

        var createdResult = await controller.Create(
            new CreateWorkspaceRequest { Name = "Family budget", OwnerEmail = "owner@example.com" });
        var created = Assert.IsType<CreatedAtActionResult>(createdResult.Result);
        var workspace = Assert.IsType<BudgetWorkspace>(created.Value);
        var invitedResult = await controller.Invite(workspace.Id,
            new InviteWorkspaceMemberRequest { Email = "child@example.com", Role = "Contributor" });
        var invited = Assert.IsType<OkObjectResult>(invitedResult.Result);

        var member = Assert.IsType<BudgetMember>(invited.Value);
        Assert.Equal("Contributor", member.Role);
        Assert.Equal(2, await context.BudgetMembers.CountAsync());
    }
}