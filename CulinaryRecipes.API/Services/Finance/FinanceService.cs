using CulinaryRecipes.API.Data.Finance;
using CulinaryRecipes.API.Models.Finance;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Services.Finance;

public class FinanceService : IFinanceService
{
    private readonly IFinanceMongoDbContext _database;

    public FinanceService(IFinanceMongoDbContext database)
    {
        _database = database;
    }

    public async Task<FinanceDashboard> GetDashboardAsync(string userId, int year, int month)
    {
        await EnsureDefaultTypesAsync(userId);
        var (start, end) = MonthRange(year, month);

        var expensesTask = _database.Expenses
            .Find(item => item.UserId == userId && item.Date >= start && item.Date < end)
            .SortByDescending(item => item.Date)
            .ToListAsync();
        var incomesTask = _database.Incomes
            .Find(item => item.UserId == userId && item.Date >= start && item.Date < end)
            .SortByDescending(item => item.Date)
            .ToListAsync();
        var expenseTypesTask = _database.ExpenseTypes
            .Find(item => item.UserId == userId)
            .SortBy(item => item.Name)
            .ToListAsync();
        var incomeTypesTask = _database.IncomeTypes
            .Find(item => item.UserId == userId)
            .SortBy(item => item.Name)
            .ToListAsync();
        var planTask = _database.MonthlyPlans
            .Find(item => item.UserId == userId && item.Year == year && item.Month == month)
            .FirstOrDefaultAsync();

        await Task.WhenAll(expensesTask, incomesTask, expenseTypesTask, incomeTypesTask, planTask);

        var expenses = await expensesTask;
        var incomes = await incomesTask;
        var plan = await planTask;
        var summary = BuildSummary(year, month, incomes.Sum(item => item.Amount), expenses.Sum(item => item.Amount), plan);

        return new FinanceDashboard(
            summary,
            expenses,
            incomes,
            await expenseTypesTask,
            await incomeTypesTask,
            plan);
    }

    public async Task<IReadOnlyList<MonthSummary>> GetYearSummaryAsync(string userId, int year)
    {
        var start = UtcDate(year, 1, 1);
        var end = start.AddYears(1);
        var expensesTask = _database.Expenses
            .Find(item => item.UserId == userId && item.Date >= start && item.Date < end)
            .ToListAsync();
        var incomesTask = _database.Incomes
            .Find(item => item.UserId == userId && item.Date >= start && item.Date < end)
            .ToListAsync();
        var plansTask = _database.MonthlyPlans
            .Find(item => item.UserId == userId && item.Year == year)
            .ToListAsync();

        await Task.WhenAll(expensesTask, incomesTask, plansTask);
        var expenses = (await expensesTask).GroupBy(item => item.Date.Month).ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));
        var incomes = (await incomesTask).GroupBy(item => item.Date.Month).ToDictionary(group => group.Key, group => group.Sum(item => item.Amount));
        var plans = (await plansTask).ToDictionary(item => item.Month);

        return Enumerable.Range(1, 12)
            .Select(month => BuildSummary(
                year,
                month,
                incomes.GetValueOrDefault(month),
                expenses.GetValueOrDefault(month),
                plans.GetValueOrDefault(month)))
            .ToList();
    }

    public async Task<Expense?> GetExpenseAsync(string userId, string id) =>
        await _database.Expenses.Find(item => item.id == id && item.UserId == userId).FirstOrDefaultAsync();

    public async Task<Expense> CreateExpenseAsync(string userId, TransactionRequest request)
    {
        await RequireExpenseTypeAsync(userId, request.TypeId);
        var expense = new Expense
        {
            UserId = userId,
            ExpenseTypeId = request.TypeId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Date = AsUtcDate(request.Date),
            CreatedAt = DateTime.UtcNow
        };
        await _database.Expenses.InsertOneAsync(expense);
        return expense;
    }

    public async Task<Expense?> UpdateExpenseAsync(string userId, string id, TransactionRequest request)
    {
        await RequireExpenseTypeAsync(userId, request.TypeId);
        var expense = await GetExpenseAsync(userId, id);
        if (expense is null) return null;

        expense.ExpenseTypeId = request.TypeId;
        expense.Description = request.Description.Trim();
        expense.Amount = request.Amount;
        expense.Date = AsUtcDate(request.Date);
        expense.UpdatedAt = DateTime.UtcNow;
        await _database.Expenses.ReplaceOneAsync(item => item.id == id && item.UserId == userId, expense);
        return expense;
    }

    public async Task<bool> DeleteExpenseAsync(string userId, string id)
    {
        var result = await _database.Expenses.DeleteOneAsync(item => item.id == id && item.UserId == userId);
        return result.DeletedCount == 1;
    }

    public async Task<Income?> GetIncomeAsync(string userId, string id) =>
        await _database.Incomes.Find(item => item.id == id && item.UserId == userId).FirstOrDefaultAsync();

    public async Task<Income> CreateIncomeAsync(string userId, TransactionRequest request)
    {
        await RequireIncomeTypeAsync(userId, request.TypeId);
        var income = new Income
        {
            UserId = userId,
            IncomeTypeId = request.TypeId,
            Description = request.Description.Trim(),
            Amount = request.Amount,
            Date = AsUtcDate(request.Date),
            CreatedAt = DateTime.UtcNow
        };
        await _database.Incomes.InsertOneAsync(income);
        return income;
    }

    public async Task<Income?> UpdateIncomeAsync(string userId, string id, TransactionRequest request)
    {
        await RequireIncomeTypeAsync(userId, request.TypeId);
        var income = await GetIncomeAsync(userId, id);
        if (income is null) return null;

        income.IncomeTypeId = request.TypeId;
        income.Description = request.Description.Trim();
        income.Amount = request.Amount;
        income.Date = AsUtcDate(request.Date);
        income.UpdatedAt = DateTime.UtcNow;
        await _database.Incomes.ReplaceOneAsync(item => item.id == id && item.UserId == userId, income);
        return income;
    }

    public async Task<bool> DeleteIncomeAsync(string userId, string id)
    {
        var result = await _database.Incomes.DeleteOneAsync(item => item.id == id && item.UserId == userId);
        return result.DeletedCount == 1;
    }

    public async Task<ExpenseType> CreateExpenseTypeAsync(string userId, FinanceTypeRequest request)
    {
        var type = new ExpenseType { UserId = userId, Name = request.Name.Trim(), Description = request.Description.Trim() };
        await _database.ExpenseTypes.InsertOneAsync(type);
        return type;
    }

    public async Task<IncomeType> CreateIncomeTypeAsync(string userId, FinanceTypeRequest request)
    {
        var type = new IncomeType { UserId = userId, Name = request.Name.Trim(), Description = request.Description.Trim() };
        await _database.IncomeTypes.InsertOneAsync(type);
        return type;
    }

    public async Task<MonthlyPlan> UpsertMonthlyPlanAsync(string userId, int year, int month, MonthlyPlanRequest request)
    {
        ValidateMonth(year, month);
        var filter = Builders<MonthlyPlan>.Filter.Where(item => item.UserId == userId && item.Year == year && item.Month == month);
        var existing = await _database.MonthlyPlans.Find(filter).FirstOrDefaultAsync();
        var plan = existing ?? new MonthlyPlan { UserId = userId, Year = year, Month = month };
        plan.ExpenseLimit = request.ExpenseLimit;
        plan.SavingsGoal = request.SavingsGoal;
        await _database.MonthlyPlans.ReplaceOneAsync(filter, plan, new ReplaceOptions { IsUpsert = true });
        return plan;
    }

    private async Task EnsureDefaultTypesAsync(string userId)
    {
        if (!await _database.ExpenseTypes.Find(item => item.UserId == userId).AnyAsync())
        {
            await _database.ExpenseTypes.InsertManyAsync(new[]
            {
                new ExpenseType { UserId = userId, Name = "Home cooking", Description = "Food and ingredients cooked at home" },
                new ExpenseType { UserId = userId, Name = "Eating out", Description = "Restaurants, cafes and takeaways" },
                new ExpenseType { UserId = userId, Name = "Housing", Description = "Rent, utilities and home costs" },
                new ExpenseType { UserId = userId, Name = "Transport", Description = "Public transport, fuel and travel" }
            });
        }

        if (!await _database.IncomeTypes.Find(item => item.UserId == userId).AnyAsync())
        {
            await _database.IncomeTypes.InsertManyAsync(new[]
            {
                new IncomeType { UserId = userId, Name = "Salary", Description = "Regular employment income" },
                new IncomeType { UserId = userId, Name = "Freelance", Description = "Contract and freelance work" },
                new IncomeType { UserId = userId, Name = "Other", Description = "Gifts, refunds and other income" }
            });
        }
    }

    private async Task RequireExpenseTypeAsync(string userId, string typeId)
    {
        if (!await _database.ExpenseTypes.Find(item => item.id == typeId && item.UserId == userId).AnyAsync())
            throw new KeyNotFoundException("Expense type was not found.");
    }

    private async Task RequireIncomeTypeAsync(string userId, string typeId)
    {
        if (!await _database.IncomeTypes.Find(item => item.id == typeId && item.UserId == userId).AnyAsync())
            throw new KeyNotFoundException("Income type was not found.");
    }

    private static MonthSummary BuildSummary(int year, int month, decimal income, decimal expenses, MonthlyPlan? plan)
    {
        var savings = income - expenses;
        var expenseLimit = plan?.ExpenseLimit ?? 0;
        var savingsGoal = plan?.SavingsGoal ?? 0;
        return new MonthSummary(
            year,
            month,
            income,
            expenses,
            savings,
            expenseLimit,
            savingsGoal,
            expenseLimit > 0 ? expenseLimit - expenses : 0,
            expenseLimit > 0 ? Math.Round(expenses / expenseLimit * 100, 1) : 0,
            savingsGoal > 0 ? Math.Round(savings / savingsGoal * 100, 1) : 0);
    }

    private static (DateTime Start, DateTime End) MonthRange(int year, int month)
    {
        ValidateMonth(year, month);
        var start = UtcDate(year, month, 1);
        return (start, start.AddMonths(1));
    }

    private static void ValidateMonth(int year, int month)
    {
        if (year is < 2000 or > 2200 || month is < 1 or > 12)
            throw new ArgumentOutOfRangeException(nameof(month), "Year or month is outside the supported range.");
    }

    private static DateTime UtcDate(int year, int month, int day) =>
        new(year, month, day, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime AsUtcDate(DateTime value) =>
        new(value.Year, value.Month, value.Day, 0, 0, 0, DateTimeKind.Utc);
}
