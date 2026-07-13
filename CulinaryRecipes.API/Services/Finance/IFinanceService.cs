using CulinaryRecipes.API.Models.Finance;

namespace CulinaryRecipes.API.Services.Finance;

public interface IFinanceService
{
    Task<FinanceDashboard> GetDashboardAsync(string userId, int year, int month);
    Task<IReadOnlyList<MonthSummary>> GetYearSummaryAsync(string userId, int year);
    Task<Expense?> GetExpenseAsync(string userId, string id);
    Task<Expense> CreateExpenseAsync(string userId, TransactionRequest request);
    Task<Expense?> UpdateExpenseAsync(string userId, string id, TransactionRequest request);
    Task<bool> DeleteExpenseAsync(string userId, string id);
    Task<Income?> GetIncomeAsync(string userId, string id);
    Task<Income> CreateIncomeAsync(string userId, TransactionRequest request);
    Task<Income?> UpdateIncomeAsync(string userId, string id, TransactionRequest request);
    Task<bool> DeleteIncomeAsync(string userId, string id);
    Task<ExpenseType> CreateExpenseTypeAsync(string userId, FinanceTypeRequest request);
    Task<IncomeType> CreateIncomeTypeAsync(string userId, FinanceTypeRequest request);
    Task<MonthlyPlan> UpsertMonthlyPlanAsync(string userId, int year, int month, MonthlyPlanRequest request);
}
