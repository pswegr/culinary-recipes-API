namespace CulinaryRecipes.API.Models.Finance;

public class FinanceDatabaseSettings
{
    public string ConnectionString { get; set; } = string.Empty;
    public string DatabaseName { get; set; } = "Finance";
    public string ExpensesCollectionName { get; set; } = "Expenses";
    public string ExpenseTypesCollectionName { get; set; } = "ExpenseTypes";
    public string IncomesCollectionName { get; set; } = "Incomes";
    public string IncomeTypesCollectionName { get; set; } = "IncomeTypes";
    public string MonthlyPlansCollectionName { get; set; } = "MonthlyPlans";
}
