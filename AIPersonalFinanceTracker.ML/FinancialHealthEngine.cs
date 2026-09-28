using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.ML;

public class FinancialHealthEngine
{
    public FinancialHealthDto CalculateHealthScore(List<Transaction> transactions, List<Category> categories)
    {
        var health = new FinancialHealthDto();
        decimal totalIncome = transactions.Where(t => t.Amount > 0).Sum(t => t.Amount);
        decimal totalExpense = transactions.Where(t => t.Amount < 0).Sum(t => Math.Abs(t.Amount));

        decimal savingsRate = totalIncome > 0 ? ((totalIncome - totalExpense) / totalIncome) * 100 : 0;
        health.SavingsRate = Math.Max(0, savingsRate);

        int score = 50;

        if (savingsRate >= 20)
        {
            score += 30;
            health.Recommendations.Add("Strong Savings Rate: You are saving over 20% of net income.");
        }
        else if (savingsRate > 0)
        {
            score += 15;
            health.Recommendations.Add("Moderate Savings: Try capping dining out to boost savings above 20%.");
        }
        else
        {
            score -= 15;
            health.Recommendations.Add("Deficit Alert: Monthly expenses exceed total income.");
        }

        var overBudgetCategories = categories.Where(c => c.Type == "Expense" && c.MonthlyBudgetLimit > 0)
            .Where(c => transactions.Where(t => t.CategoryId == c.Id && t.Amount < 0).Sum(t => Math.Abs(t.Amount)) > c.MonthlyBudgetLimit)
            .ToList();

        if (!overBudgetCategories.Any())
        {
            score += 20;
            health.Recommendations.Add("Budget Adherence: All expense categories are within designated limits.");
        }
        else
        {
            score -= 10;
            foreach (var cat in overBudgetCategories)
            {
                health.Recommendations.Add($"Over Budget: '{cat.Name}' spending exceeds monthly budget limits.");
            }
        }

        health.HealthScore = Math.Clamp(score, 0, 100);
        health.HealthGrade = health.HealthScore >= 80 ? "A" : health.HealthScore >= 60 ? "B" : "C";

        return health;
    }
}
