using CulinaryRecipes.API.Models.Finance;
using CulinaryRecipes.API.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace CulinaryRecipes.API.Data.Finance;

public class FinanceMongoDbContext : IFinanceMongoDbContext
{
    public FinanceMongoDbContext(
        IOptions<FinanceDatabaseSettings> settings,
        IOptions<CulinaryRecipesDatabaseSettings> mainDatabaseSettings)
    {
        var connectionString = string.IsNullOrWhiteSpace(settings.Value.ConnectionString)
            ? mainDatabaseSettings.Value.ConnectionString
            : settings.Value.ConnectionString;

        var database = new MongoClient(connectionString)
            .GetDatabase(settings.Value.DatabaseName);

        Expenses = database.GetCollection<Expense>(settings.Value.ExpensesCollectionName);
        ExpenseTypes = database.GetCollection<ExpenseType>(settings.Value.ExpenseTypesCollectionName);
        Incomes = database.GetCollection<Income>(settings.Value.IncomesCollectionName);
        IncomeTypes = database.GetCollection<IncomeType>(settings.Value.IncomeTypesCollectionName);
        MonthlyPlans = database.GetCollection<MonthlyPlan>(settings.Value.MonthlyPlansCollectionName);
    }

    public IMongoCollection<Expense> Expenses { get; }
    public IMongoCollection<ExpenseType> ExpenseTypes { get; }
    public IMongoCollection<Income> Incomes { get; }
    public IMongoCollection<IncomeType> IncomeTypes { get; }
    public IMongoCollection<MonthlyPlan> MonthlyPlans { get; }
}
