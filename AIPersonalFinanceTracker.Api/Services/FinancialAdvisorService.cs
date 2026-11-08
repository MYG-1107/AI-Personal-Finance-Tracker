using AIPersonalFinanceTracker.Shared.Models;

namespace AIPersonalFinanceTracker.Api.Services;

public class FinancialAdvisorService
{
    public AdvisorResponse Answer(
        string question,
        IReadOnlyCollection<Transaction> transactions,
        IReadOnlyDictionary<int, string> categories,
        IReadOnlyCollection<SavingsGoal> goals)
    {
        if (transactions.Count == 0)
            return new AdvisorResponse { Answer = "Add a few transactions and I will turn them into personalized insights." };

        var normalizedQuestion = question.Trim().ToLowerInvariant();
        var expenses = transactions.Where(transaction => transaction.Amount < 0).ToList();
        var totalSpend = expenses.Sum(transaction => Math.Abs(transaction.Amount));
        var income = transactions.Where(transaction => transaction.Amount > 0).Sum(transaction => transaction.Amount);
        var categoryTotals = expenses
            .GroupBy(transaction => transaction.CategoryId)
            .Select(group => new
            {
                CategoryId = group.Key,
                Total = group.Sum(transaction => Math.Abs(transaction.Amount))
            })
            .OrderByDescending(item => item.Total)
            .ToList();
        var top = categoryTotals.FirstOrDefault();
        var threeMonthStart = DateTime.UtcNow.AddMonths(-3);
        var recentExpenses = expenses.Where(transaction => transaction.Date >= threeMonthStart).ToList();

        if (normalizedQuestion.Contains("average") && normalizedQuestion.Contains("month"))
        {
            var average = recentExpenses.Sum(transaction => Math.Abs(transaction.Amount)) / 3m;
            return Response(
                $"Your average monthly spending over the last three months is {average:N2}.",
                $"Tracked spending in that period: {recentExpenses.Sum(transaction => Math.Abs(transaction.Amount)):N2}",
                $"Average monthly income in all tracked data: {income:N2}");
        }

        if (normalizedQuestion.Contains("goal") || normalizedQuestion.Contains("saving"))
        {
            var activeGoals = goals.Where(goal => goal.TargetAmount > goal.CurrentAmount).ToList();
            var goalSummary = activeGoals.Count == 0
                ? "You have no active savings gap recorded."
                : string.Join("; ", activeGoals.Select(goal =>
                    $"{goal.Name} needs {(goal.TargetAmount - goal.CurrentAmount):N2} more"));
            return Response(goalSummary, $"Total expenses: {totalSpend:N2}", $"Tracked income: {income:N2}");
        }

        if (normalizedQuestion.Contains("compare") || normalizedQuestion.Contains("more") && normalizedQuestion.Contains("month"))
        {
            var currentMonth = expenses.Where(transaction => transaction.Date.Month == DateTime.UtcNow.Month &&
                transaction.Date.Year == DateTime.UtcNow.Year).Sum(transaction => Math.Abs(transaction.Amount));
            var previousDate = DateTime.UtcNow.AddMonths(-1);
            var previousMonth = expenses.Where(transaction => transaction.Date.Month == previousDate.Month &&
                transaction.Date.Year == previousDate.Year).Sum(transaction => Math.Abs(transaction.Amount));
            var direction = currentMonth >= previousMonth ? "more" : "less";
            return Response(
                $"This month you spent {currentMonth:N2}, which is {direction} than last month ({previousMonth:N2}).",
                $"Difference: {Math.Abs(currentMonth - previousMonth):N2}",
                $"Three-month tracked spend: {recentExpenses.Sum(transaction => Math.Abs(transaction.Amount)):N2}");
        }

        var categoryName = top is null ? "Uncategorized" : GetCategoryName(top.CategoryId, categories);
        return Response(
            $"Your largest spending area is {categoryName}, totaling {top?.Total ?? 0:N2}.",
            $"Total tracked spending: {totalSpend:N2}",
            $"Transactions retrieved for this answer: {transactions.Count}");
    }

    private static AdvisorResponse Response(string answer, params string[] highlights) => new()
    {
        Answer = answer,
        Highlights = highlights.ToList()
    };

    private static string GetCategoryName(int? categoryId, IReadOnlyDictionary<int, string> categories) =>
        categoryId.HasValue && categories.TryGetValue(categoryId.Value, out var name) ? name : "Uncategorized";
}