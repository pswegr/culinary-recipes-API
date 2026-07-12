using CulinaryRecipes.API.Models.Finance;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Finance;

public interface IFinanceMongoDbContext
{
    IMongoCollection<Expense> Expenses { get; }
    IMongoCollection<ExpenseType> ExpenseTypes { get; }
    IMongoCollection<Income> Incomes { get; }
    IMongoCollection<IncomeType> IncomeTypes { get; }
    IMongoCollection<MonthlyPlan> MonthlyPlans { get; }
}
