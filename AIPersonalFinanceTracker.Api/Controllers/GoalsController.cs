using AIPersonalFinanceTracker.Api.Data;
using AIPersonalFinanceTracker.Shared.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AIPersonalFinanceTracker.Api.Controllers;

[ApiController]
[Route("api/goals")]
public class GoalsController : ControllerBase
{
    private readonly AppDbContext _context;

    public GoalsController(AppDbContext context) => _context = context;

    [HttpGet]
    public async Task<ActionResult<List<SavingsGoal>>> GetGoals() =>
        Ok(await _context.SavingsGoals.AsNoTracking().OrderBy(g => g.TargetDate).ToListAsync());

    [HttpPost]
    public async Task<IActionResult> CreateGoal(SavingsGoal goal)
    {
        if (goal == null || string.IsNullOrWhiteSpace(goal.Name))
            return BadRequest(new { message = "A goal name is required." });
        if (goal.TargetAmount <= 0 || goal.CurrentAmount < 0 || goal.CurrentAmount > goal.TargetAmount)
            return BadRequest(new { message = "Goal amounts must be valid and within the target." });

        goal.Id = 0;
        goal.Name = goal.Name.Trim();
        _context.SavingsGoals.Add(goal);
        await _context.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateGoal(int id, SavingsGoal input)
    {
        var goal = await _context.SavingsGoals.FindAsync(id);
        if (goal == null) return NotFound(new { message = "Goal not found." });
        if (input.TargetAmount <= 0 || input.CurrentAmount < 0 || input.CurrentAmount > input.TargetAmount)
            return BadRequest(new { message = "Goal amounts must be valid and within the target." });

        goal.Name = input.Name.Trim();
        goal.TargetAmount = input.TargetAmount;
        goal.CurrentAmount = input.CurrentAmount;
        goal.TargetDate = input.TargetDate;
        goal.Color = input.Color;
        await _context.SaveChangesAsync();
        return Ok(goal);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGoal(int id)
    {
        var goal = await _context.SavingsGoals.FindAsync(id);
        if (goal == null) return NotFound();
        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
